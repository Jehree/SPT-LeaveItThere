import { DependencyContainer } from "tsyringe";
import { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";
import { FileUtils, InitStage, ModHelper } from "./mod_helper";
import * as fs from "fs";
import Config from "../config.json";
import { BaseClasses } from "@spt/models/enums/BaseClasses";
import path from "path";

class Mod implements IPreSptLoadMod, IPostDBLoadMod {
    public Helper = new ModHelper();

    public DataToServer = "/jehree/pip/data_to_server";
    public DataToClient = "/jehree/pip/data_to_client";

    public preSptLoad(container: DependencyContainer): void {
        this.Helper.init(container, InitStage.PRE_SPT_LOAD);

        this.Helper.registerStaticRoute(this.DataToServer, "LeaveItThere-DataToServer", Mod.onDataToServer, Mod);
        this.Helper.registerStaticRoute(this.DataToClient, "LeaveItThere-DataToClient", Mod.onDataToClient, Mod, true);

        //item_data migration (should probably remove in a couple weeks):
        this.Helper.registerStaticRoute(
            "/client/game/start",
            "LeaveItThere-ProfileMigrationRoute",
            (url: string, info: any, sessionId: string, output: string, helper: ModHelper) => {
                const oldFolderPath: string = FileUtils.pathCombine(ModHelper.modPath, "item_data");
                if (!fs.existsSync(oldFolderPath)) return;
                const entries = fs.readdirSync(oldFolderPath, { withFileTypes: true });
                const files: string[] = entries.filter((entry) => entry.isFile()).map((entry) => path.join(oldFolderPath, entry.name));

                for (const filePath of files) {
                    if (path.extname(filePath) != ".json") continue;

                    const data = JSON.parse(fs.readFileSync(filePath, "utf8"));
                    data["ProfileId"] = sessionId;

                    const newFilePath: string = Mod.getProfileDataPath(sessionId, data.MapId);
                    fs.writeFileSync(newFilePath, JSON.stringify(data));
                }

                fs.renameSync(oldFolderPath, oldFolderPath + "_OLD");
            }
        );
    }

    public postDBLoad(container: DependencyContainer): void {
        this.Helper.init(container, InitStage.POST_DB_LOAD);
        if (Config.remove_in_raid_restrictions) {
            this.Helper.dbGlobals.config.RestrictionsInRaid = [];
        }
        if (Config.everything_is_discardable) {
        }
        for (const [_, item] of Object.entries(this.Helper.dbItems)) {
            if (item._type !== "Item") continue;

            if (Config.everything_is_discardable) {
                item._props.DiscardLimit = -1;
            }

            if (Config.remove_backpack_restrictions && this.Helper.itemHelper.isOfBaseclass(item._id, BaseClasses.BACKPACK)) {
                for (const [_, grid] of Object.entries(item._props.Grids)) {
                    if (!grid?._props?.filters) continue;
                    grid._props.filters = [
                        {
                            Filter: [BaseClasses.ITEM],
                            ExcludedFilter: [],
                        },
                    ];
                }
            }
        }
    }

    public static onDataToServer(url: string, info: any, sessionId: string, output: string, helper: ModHelper): void {
        const data = JSON.parse(JSON.stringify(info));
        const mapId: string = data.MapId;
        const profileId: string = data.ProfileId;

        this.makeBackup(profileId);

        const path: string = this.getProfileDataPath(profileId, mapId);
        fs.writeFileSync(path, JSON.stringify(info));
    }

    public static onDataToClient(url: string, info: any, sessionId: string, output: string, helper: ModHelper): string {
        const data = JSON.parse(JSON.stringify(info));
        const mapId: string = data.MapId;
        const profileId: string = data.ProfileId;
        const path: string = this.getProfileDataPath(profileId, mapId);
        if (!fs.existsSync(path)) {
            return `{"ProfileId": "${profileId}", "MapId": "${mapId}", "ItemTemplates": []}`;
        } else {
            return fs.readFileSync(path, "utf8");
        }
    }

    public static getProfileFolderPath(profileId: string): string {
        let profileFolderName: string = Config.global_item_data_profile ? "global" : profileId;
        const folderPath: string = FileUtils.pathCombine(ModHelper.profilePath, "LeaveItThere-ItemData", profileFolderName);
        return folderPath;
    }

    public static getProfileDataPath(profileId: string, mapId: string): string {
        let mapName: string = mapId;
        if (mapId === "factory4_day" || mapId === "factory4_night") {
            mapName = "factory";
        }
        if (mapId === "Sandbox_high") {
            mapName = "Sandbox";
        }

        const folderPath: string = this.getProfileFolderPath(profileId);
        const filePath: string = FileUtils.pathCombine(folderPath, `${mapName}.json`);
        fs.mkdirSync(folderPath, { recursive: true });

        return filePath;
    }

    public static makeBackup(profileId: string): void {
        if (Config.max_profile_backup_count < 0) {
            console.error(
                "\x1b[31m%s\x1b[0m",
                "[HomeComforts]: max_profile_backup_count in config.json must be a number that is 0 or greater! Fix this or auto profile backups will not work!"
            );
            return;
        }
        const profileFolderPath = this.getProfileFolderPath(profileId);
        const backupsFolderPath: string = FileUtils.pathCombine(profileFolderPath, "backups");
        const thisBackupFolderPath: string = FileUtils.pathCombine(backupsFolderPath, this.getTimestamp());
        fs.mkdirSync(thisBackupFolderPath, { recursive: true });

        const jsonFiles: fs.Dirent[] = fs.readdirSync(profileFolderPath, { withFileTypes: true });

        for (const file of jsonFiles) {
            if (!file.isFile()) continue;
            if (!file.name.endsWith(".json")) continue;

            const jsonPath: string = FileUtils.pathCombine(profileFolderPath, file.name);
            const destinationPath: string = FileUtils.pathCombine(thisBackupFolderPath, file.name);

            fs.copyFileSync(jsonPath, destinationPath);
        }

        this.cullOldBackups(backupsFolderPath);
    }

    public static cullOldBackups(backupsFolderPath: string): void {
        const items = fs.readdirSync(backupsFolderPath, { withFileTypes: true });
        const backupFolderPaths: string[] = items
            .filter((item) => item.isDirectory())
            .map((item) => FileUtils.pathCombine(backupsFolderPath, item.name));

        if (backupFolderPaths.length === 0) return;
        if (backupFolderPaths.length <= Config.max_profile_backup_count) return;

        let oldestFolder = backupFolderPaths[0];
        let oldestTime = fs.statSync(oldestFolder).birthtime.getTime();

        for (const folder of backupFolderPaths.slice(1)) {
            const folderTime = fs.statSync(folder).birthtime.getTime();
            if (folderTime < oldestTime) {
                oldestFolder = folder;
                oldestTime = folderTime;
            }
        }

        fs.rmSync(oldestFolder, { recursive: true });

        // recursively call self again so that backups continue to get culled until there are only the max amount in config left
        this.cullOldBackups(backupsFolderPath);
    }

    public static getTimestamp(): string {
        const now = new Date();
        const year = now.getFullYear();
        const month = String(now.getMonth() + 1).padStart(2, "0"); // Months are 0-based
        const day = String(now.getDate()).padStart(2, "0");
        const hours = String(now.getHours()).padStart(2, "0");
        const minutes = String(now.getMinutes()).padStart(2, "0");
        const seconds = String(now.getSeconds()).padStart(2, "0");

        return `${year}-${month}-${day}_${hours}-${minutes}-${seconds}`;
    }
}

export const mod = new Mod();
