import { DependencyContainer } from "tsyringe";
import { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";
import { FileUtils, InitStage, ModHelper } from "./mod_helper";
import * as fs from "fs";
import { LogTextColor } from "@spt/models/spt/logging/LogTextColor";
import Config from "../config.json";
import { BaseClasses } from "@spt/models/enums/BaseClasses";
import { HealthHelper } from "@spt/helpers/HealthHelper";
import path from "path";
import { config } from "process";

class Mod implements IPreSptLoadMod, IPostDBLoadMod {
    public Helper = new ModHelper();

    public DataToServer = "/jehree/pip/data_to_server";
    public DataToClient = "/jehree/pip/data_to_client";

    public preSptLoad(container: DependencyContainer): void {
        this.Helper.init(container, InitStage.PRE_SPT_LOAD);

        this.Helper.registerStaticRoute(this.DataToServer, "LeaveItThere-DataToServer", Routes.onDataToServer, Routes);
        this.Helper.registerStaticRoute(this.DataToClient, "LeaveItThere-DataToClient", Routes.onDataToClient, Routes, true);

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

                    const newFilePath: string = Routes.getPath(sessionId, data.MapId);
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
}

export class Routes {
    public static onDataToServer(url: string, info: any, sessionId: string, output: string, helper: ModHelper): void {
        const data = JSON.parse(JSON.stringify(info));
        const mapId: string = data.MapId;
        const profileId: string = data.ProfileId;
        const path: string = this.getPath(profileId, mapId);
        fs.writeFileSync(path, JSON.stringify(info));
    }

    public static onDataToClient(url: string, info: any, sessionId: string, output: string, helper: ModHelper): string {
        const data = JSON.parse(JSON.stringify(info));
        const mapId: string = data.MapId;
        const profileId: string = data.ProfileId;
        const path: string = this.getPath(profileId, mapId);
        if (!fs.existsSync(path)) {
            return `{"ProfileId": "${profileId}", "MapId": "${mapId}", "ItemTemplates": []}`;
        } else {
            return fs.readFileSync(path, "utf8");
        }
    }

    public static getPath(profileId: string, mapId: string): string {
        let mapName: string = mapId;
        if (mapId === "factory4_day" || mapId === "factory4_night") {
            mapName = "factory";
        }
        if (mapId === "Sandbox_high") {
            mapName = "Sandbox";
        }
        let profileName: string = profileId;
        if (Config.global_item_data_profile) {
            profileName = "global";
        }

        const folderPath: string = FileUtils.pathCombine(ModHelper.profilePath, "LeaveItThere-ItemData", profileName);
        const filePath: string = FileUtils.pathCombine(folderPath, `${mapName}.json`);
        fs.mkdirSync(folderPath, { recursive: true });

        return filePath;
    }
}

export const mod = new Mod();
