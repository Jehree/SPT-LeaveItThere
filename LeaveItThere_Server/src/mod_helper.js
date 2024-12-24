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
exports.MongoMapperSingleton = exports.ModHelper = exports.FileUtils = exports.CurrencyId = exports.TraderId = exports.InitStage = void 0;
const fs = __importStar(require("fs"));
const path = __importStar(require("path"));
const LogTextColor_1 = require("C:/snapshot/project/obj/models/spt/logging/LogTextColor");
var InitStage;
(function (InitStage) {
    InitStage[InitStage["PRE_SPT_LOAD"] = 0] = "PRE_SPT_LOAD";
    InitStage[InitStage["POST_DB_LOAD"] = 1] = "POST_DB_LOAD";
    InitStage[InitStage["ALL"] = 2] = "ALL";
})(InitStage || (exports.InitStage = InitStage = {}));
var TraderId;
(function (TraderId) {
    TraderId["MECHANIC"] = "5a7c2eca46aef81a7ca2145d";
    TraderId["SKIER"] = "58330581ace78e27b8b10cee";
    TraderId["PEACEKEEPER"] = "5935c25fb3acc3127c3d8cd9";
    TraderId["THERAPIST"] = "54cb57776803fa99248b456e";
    TraderId["PRAPOR"] = "54cb50c76803fa8b248b4571";
    TraderId["JAEGER"] = "5c0647fdd443bc2504c2d371";
    TraderId["RAGMAN"] = "5ac3b934156ae10c4430e83c";
    TraderId["FENCE"] = "579dc571d53a0658a154fbec";
})(TraderId || (exports.TraderId = TraderId = {}));
var CurrencyId;
(function (CurrencyId) {
    CurrencyId["RUB"] = "5449016a4bdc2d6f028b456f";
    CurrencyId["EUR"] = "569668774bdc2da2298b4568";
    CurrencyId["USD"] = "5696686a4bdc2da3298b456a";
})(CurrencyId || (exports.CurrencyId = CurrencyId = {}));
class FileUtils {
    static readJson(targetPath, useRawPath = false) {
        let filePath;
        if (useRawPath) {
            filePath = targetPath;
        }
        else {
            filePath = path.resolve(__dirname, targetPath);
        }
        try {
            const file = fs.readFileSync(filePath, "utf8");
            return JSON.parse(file);
        }
        catch (err) {
            if (err instanceof Error) {
                console.log(err);
                console.log(`Tried to read json at invalid path: ${path}`);
            }
        }
    }
    static writeJson(data, targetPath, useRawPath = false) {
        let filePath;
        let jsonString;
        if (typeof data === "string") {
            jsonString = data;
        }
        else {
            jsonString = JSON.stringify(data, null, 4);
        }
        if (useRawPath) {
            filePath = targetPath;
        }
        else {
            filePath = path.resolve(__dirname, targetPath);
        }
        try {
            fs.writeFileSync(filePath, jsonString, "utf8");
        }
        catch (err) {
            if (err instanceof Error) {
                console.log(err);
                console.log(`Tried to write json at invalid path: ${path}`);
            }
        }
    }
    static pathCombine(...paths) {
        return path.normalize(path.join(...paths));
    }
    static jsonClone(data) {
        let jsonString;
        if (typeof data === "string") {
            jsonString = data;
        }
        else {
            jsonString = JSON.stringify(data, null, 4);
        }
        if (typeof data === "string")
            return jsonString;
        return JSON.parse(jsonString);
    }
}
exports.FileUtils = FileUtils;
class ModHelper {
    static modName = "BOOBS";
    //useful paths
    static modPath = FileUtils.pathCombine(__dirname, "..");
    static profilePath = FileUtils.pathCombine(__dirname, "..", "..", "..", "profiles");
    static modsFolderPath = FileUtils.pathCombine(__dirname, "..", "..");
    static traderIdsByName = {
        mechanic: TraderId.MECHANIC,
        skier: TraderId.SKIER,
        peacekeeper: TraderId.PEACEKEEPER,
        therapist: TraderId.THERAPIST,
        prapor: TraderId.PRAPOR,
        jaeger: TraderId.JAEGER,
        ragman: TraderId.RAGMAN,
        fence: TraderId.FENCE,
    };
    static currencyIdsByName = {
        rub: CurrencyId.RUB,
        eur: CurrencyId.EUR,
        usd: CurrencyId.USD,
    };
    //initialized at preSptLoad
    container;
    preSptModLoader;
    imageRouter;
    configServer;
    saveServer;
    itemHelper;
    logger;
    staticRouter;
    vfs;
    hashUtil;
    inventoryHelper;
    lootGenerator;
    eventOutputHolder;
    //initialized at postDBLoad
    dbTables;
    dbGlobals;
    dbItems;
    dbQuests;
    dbTraders;
    dbLocales;
    dbHandbook;
    dbLocations;
    customItem;
    jsonUtil;
    profileHelper;
    ragfairPriceService;
    importerUtil;
    botGeneratorHelper;
    init(container, initStage) {
        if (initStage === InitStage.PRE_SPT_LOAD || initStage === InitStage.ALL) {
            this.container = container;
            this.preSptModLoader = container.resolve("PreSptModLoader");
            this.imageRouter = container.resolve("ImageRouter");
            this.configServer = container.resolve("ConfigServer");
            this.saveServer = container.resolve("SaveServer");
            this.itemHelper = container.resolve("ItemHelper");
            this.logger = container.resolve("WinstonLogger");
            this.staticRouter = container.resolve("StaticRouterModService");
            this.vfs = container.resolve("VFS");
            this.hashUtil = container.resolve("HashUtil");
            this.itemHelper = container.resolve("ItemHelper");
            this.inventoryHelper = container.resolve("InventoryHelper");
            this.lootGenerator = container.resolve("LootGenerator");
            this.eventOutputHolder = container.resolve("EventOutputHolder");
        }
        if (initStage === InitStage.POST_DB_LOAD || initStage === InitStage.ALL) {
            this.dbTables = container.resolve("DatabaseServer").getTables();
            this.dbGlobals = this.dbTables.globals;
            this.dbItems = this.dbTables.templates.items;
            this.dbQuests = this.dbTables.templates.quests;
            this.dbTraders = this.dbTables.traders;
            this.dbLocales = this.dbTables.locales;
            this.dbHandbook = this.dbTables.templates.handbook;
            this.dbLocations = this.dbTables.locations;
            this.customItem = container.resolve("CustomItemService");
            this.jsonUtil = container.resolve("JsonUtil");
            this.profileHelper = container.resolve("ProfileHelper");
            this.ragfairPriceService = container.resolve("RagfairPriceService");
            this.importerUtil = container.resolve("ImporterUtil");
            this.botGeneratorHelper = container.resolve("BotGeneratorHelper");
        }
    }
    registerStaticRoute(routeURL, routeName, callable, boundClass = undefined, // any call to 'this' in callable will call boundClass
    outputModified = false) {
        if (boundClass) {
            callable = callable.bind(boundClass);
        }
        this.staticRouter.registerStaticRouter(routeName, [
            {
                url: routeURL,
                action: async (url, info, sessionId, output) => {
                    const Helper = new ModHelper();
                    Helper.init(this.container, InitStage.ALL);
                    const modifiedOutput = callable(url, info, sessionId, output, Helper);
                    if (outputModified) {
                        return modifiedOutput;
                    }
                    else {
                        return output || JSON.stringify({ value_not_needed: true });
                    }
                    return output;
                },
            },
        ], "aki");
    }
    log(logText, logColor = LogTextColor_1.LogTextColor.WHITE, dontLogModName = false) {
        switch (dontLogModName) {
            case true: {
                this.logger.log(logText, logColor);
                break;
            }
            case false: {
                this.logger.log(`[${ModHelper.modName}]: ${logText}`, logColor);
                break;
            }
        }
    }
}
exports.ModHelper = ModHelper;
class MongoMapperSingleton {
    // this class must use the singleton pattern to avoid multiple separate instances being created
    // this is to avoid issues when multiple instances exist and are not in sync with one another
    static _instance;
    _mappingsPath = FileUtils.pathCombine(ModHelper.modPath, "src", "mongo_mappings", "DO_NOT_EDIT.json");
    _hashUtil;
    _mongoMap;
    constructor(container) {
        this._hashUtil = container.resolve("HashUtil");
        const mappingsFolderPath = FileUtils.pathCombine(ModHelper.modPath, "src", "mongo_mappings");
        if (!fs.existsSync(mappingsFolderPath)) {
            fs.mkdirSync(mappingsFolderPath);
        }
        if (!fs.existsSync(this._mappingsPath)) {
            this._mongoMap = {};
            FileUtils.writeJson(this._mongoMap, this._mappingsPath);
        }
        this._mongoMap = FileUtils.readJson(this._mappingsPath);
    }
    static getInstance(container) {
        if (!this._instance) {
            this._instance = new MongoMapperSingleton(container);
        }
        return this._instance;
    }
    getMongoId(humanReadableId) {
        if (!(humanReadableId in this._mongoMap)) {
            const mongoId = this._hashUtil.generate();
            this._mongoMap[humanReadableId] = mongoId;
            FileUtils.writeJson(this._mongoMap, this._mappingsPath);
            return mongoId;
        }
        return this._mongoMap[humanReadableId];
    }
}
exports.MongoMapperSingleton = MongoMapperSingleton;
//# sourceMappingURL=mod_helper.js.map