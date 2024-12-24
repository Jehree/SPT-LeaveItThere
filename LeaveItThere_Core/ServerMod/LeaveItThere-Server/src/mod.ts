import { DependencyContainer } from "tsyringe";
import { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";
import { FileUtils, InitStage, ModHelper } from "./mod_helper";
import * as fs from "fs";
import { LogTextColor } from "@spt/models/spt/logging/LogTextColor";
import Config from "../config.json";
import { BaseClasses } from "@spt/models/enums/BaseClasses";

class Mod implements IPreSptLoadMod, IPostDBLoadMod {
    public Helper = new ModHelper();

    public DataToServer = "/jehree/pip/data_to_server";
    public DataToClient = "/jehree/pip/data_to_client";

    public preSptLoad(container: DependencyContainer): void {
        this.Helper.init(container, InitStage.PRE_SPT_LOAD);

        this.Helper.registerStaticRoute(this.DataToServer, "LeaveItThere-DataToServer", Routes.onDataToServer, Routes);
        this.Helper.registerStaticRoute(this.DataToClient, "LeaveItThere-DataToClient", Routes.onDataToClient, Routes, true);
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
        const data: string = JSON.stringify(info);
        const mapId: string = JSON.parse(data).MapId;
        const path: string = this.getPath(mapId);
        fs.writeFileSync(path, data);
    }

    public static onDataToClient(url: string, info: any, sessionId: string, output: string, helper: ModHelper): string {
        const data: string = JSON.stringify(info);
        const mapId: string = JSON.parse(data).MapId;
        const path: string = this.getPath(mapId);
        if (!fs.existsSync(path)) {
            return `{"MapId": "${mapId}", "ItemTemplates": []}`;
        } else {
            return fs.readFileSync(path, "utf8");
        }
    }

    public static getPath(mapId: string): string {
        let fileName: string = mapId;
        if (mapId === "factory4_day" || mapId === "factory4_night") {
            fileName = "factory";
        }
        if (mapId === "sandbox_high") {
            fileName = "sandbox";
        }

        return FileUtils.pathCombine(ModHelper.modPath, "item_data", `${fileName}.json`);
    }
}

export const mod = new Mod();
