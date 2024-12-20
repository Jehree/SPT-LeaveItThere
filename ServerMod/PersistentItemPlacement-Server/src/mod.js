"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.mod = exports.Routes = void 0;
const mod_helper_1 = require("./mod_helper");
const fs = __importStar(require("fs"));
class Mod {
    Helper = new mod_helper_1.ModHelper();
    DataToServer = "/jehree/pip/data_to_server";
    DataToClient = "/jehree/pip/data_to_client";
    preSptLoad(container) {
        this.Helper.init(container, mod_helper_1.InitStage.PRE_SPT_LOAD);
        this.Helper.registerStaticRoute(this.DataToServer, "PersistentItemPlacement-DataToServer", Routes.onDataToServer, Routes);
        this.Helper.registerStaticRoute(this.DataToClient, "PersistentItemPlacement-DataToClient", Routes.onDataToClient, Routes, true);
    }
}
class Routes {
    static onDataToServer(url, info, sessionId, output, helper) {
        const data = JSON.stringify(info);
        const mapId = JSON.parse(data).MapId;
        const path = this.getPath(mapId);
        fs.writeFileSync(path, data);
    }
    static onDataToClient(url, info, sessionId, output, helper) {
        const data = JSON.stringify(info);
        const mapId = JSON.parse(data).MapId;
        const path = this.getPath(mapId);
        if (!fs.existsSync(path)) {
            return `{"MapId": "${mapId}", "ItemTemplates": []}`;
        }
        else {
            return fs.readFileSync(path, "utf8");
        }
    }
    static getPath(mapId) {
        let fileName = mapId;
        if (mapId === "factory4_day" || mapId === "factory4_night") {
            fileName = "factory";
        }
        if (mapId === "sandbox_high") {
            fileName = "sandbox";
        }
        return mod_helper_1.FileUtils.pathCombine(mod_helper_1.ModHelper.modPath, "item_data", `${fileName}.json`);
    }
}
exports.Routes = Routes;
exports.mod = new Mod();
//# sourceMappingURL=mod.js.map