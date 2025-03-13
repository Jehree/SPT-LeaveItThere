import * as fs from "fs";
import * as path from "path";
import { ILogger } from "@spt/models/spt/utils/ILogger";
import { DatabaseServer } from "@spt/servers/DatabaseServer";
import { IDatabaseTables } from "@spt/models/spt/server/IDatabaseTables";
import { StaticRouterModService } from "@spt/services/mod/staticRouter/StaticRouterModService";
import { DependencyContainer } from "tsyringe";
import { CustomItemService } from "@spt/services/mod/CustomItemService";
import { ImageRouter } from "@spt/routers/ImageRouter";
import { PreSptModLoader } from "@spt/loaders/PreSptModLoader";
import { ConfigServer } from "@spt/servers/ConfigServer";
import { JsonUtil } from "@spt/utils/JsonUtil";
import { ProfileHelper } from "@spt/helpers/ProfileHelper";
import { RagfairPriceService } from "@spt/services/RagfairPriceService";
import { ImporterUtil } from "@spt/utils/ImporterUtil";
import { SaveServer } from "@spt/servers/SaveServer";
import { ItemHelper } from "@spt/helpers/ItemHelper";
import { ITemplateItem } from "@spt/models/eft/common/tables/ITemplateItem";
import { IQuest } from "@spt/models/eft/common/tables/IQuest";
import { ILocaleBase } from "@spt/models/spt/server/ILocaleBase";
import { FileSystem } from "@spt/utils/FileSystem";
import { BotGeneratorHelper } from "@spt/helpers/BotGeneratorHelper";
import { HashUtil } from "@spt/utils/HashUtil";
import { ITrader } from "@spt/models/eft/common/tables/ITrader";
import { LogTextColor } from "@spt/models/spt/logging/LogTextColor";
import { IGlobals } from "@spt/models/eft/common/IGlobals";
import { IHandbookBase } from "@spt/models/eft/common/tables/IHandbookBase";
import { InventoryHelper } from "@spt/helpers/InventoryHelper";
import { LootGenerator } from "@spt/generators/LootGenerator";
import { EventOutputHolder } from "@spt/routers/EventOutputHolder";
import { ILocations } from "@spt/models/spt/server/ILocations";

export enum InitStage {
    PRE_SPT_LOAD,
    POST_DB_LOAD,
    ALL,
}

export enum TraderId {
    MECHANIC = "5a7c2eca46aef81a7ca2145d",
    SKIER = "58330581ace78e27b8b10cee",
    PEACEKEEPER = "5935c25fb3acc3127c3d8cd9",
    THERAPIST = "54cb57776803fa99248b456e",
    PRAPOR = "54cb50c76803fa8b248b4571",
    JAEGER = "5c0647fdd443bc2504c2d371",
    RAGMAN = "5ac3b934156ae10c4430e83c",
    FENCE = "579dc571d53a0658a154fbec",
}

export enum CurrencyId {
    RUB = "5449016a4bdc2d6f028b456f",
    EUR = "569668774bdc2da2298b4568",
    USD = "5696686a4bdc2da3298b456a",
}

export class FileUtils {
    static readJson<T>(targetPath: string, useRawPath: boolean = false): T {
        let filePath: string;

        if (useRawPath) {
            filePath = targetPath;
        } else {
            filePath = path.resolve(__dirname, targetPath);
        }

        try {
            const file = fs.readFileSync(filePath, "utf8");

            return JSON.parse(file) as T;
        } catch (err: unknown) {
            if (err instanceof Error) {
                console.log(err);

                console.log(`Tried to read json at invalid path: ${path}`);
            }
        }
    }

    static writeJson(data: object, targetPath: string, useRawPath: boolean = false): void {
        let filePath: string;
        let jsonString: string;

        if (typeof data === "string") {
            jsonString = data as string;
        } else {
            jsonString = JSON.stringify(data, null, 4);
        }

        if (useRawPath) {
            filePath = targetPath;
        } else {
            filePath = path.resolve(__dirname, targetPath);
        }

        try {
            fs.writeFileSync(filePath, jsonString, "utf8");
        } catch (err: unknown) {
            if (err instanceof Error) {
                console.log(err);

                console.log(`Tried to write json at invalid path: ${path}`);
            }
        }
    }

    static pathCombine(...paths: string[]): string {
        return path.normalize(path.join(...paths));
    }

    static jsonClone<T>(data: object): T {
        let jsonString: string;

        if (typeof data === "string") {
            jsonString = data as string;
        } else {
            jsonString = JSON.stringify(data, null, 4);
        }

        if (typeof data === "string") return jsonString as T;
        return JSON.parse(jsonString) as T;
    }
}

export class ModHelper {
    public static modName: string = "LeaveItThere";

    //useful paths
    public static modPath: string = FileUtils.pathCombine(__dirname, "..");
    public static profilePath: string = FileUtils.pathCombine(__dirname, "..", "..", "..", "profiles");
    public static modsFolderPath: string = FileUtils.pathCombine(__dirname, "..", "..");

    public static traderIdsByName: Record<string, TraderId> = {
        mechanic: TraderId.MECHANIC,
        skier: TraderId.SKIER,
        peacekeeper: TraderId.PEACEKEEPER,
        therapist: TraderId.THERAPIST,
        prapor: TraderId.PRAPOR,
        jaeger: TraderId.JAEGER,
        ragman: TraderId.RAGMAN,
        fence: TraderId.FENCE,
    };

    public static currencyIdsByName: Record<string, CurrencyId> = {
        rub: CurrencyId.RUB,
        eur: CurrencyId.EUR,
        usd: CurrencyId.USD,
    };

    //initialized at preSptLoad
    public container: DependencyContainer;
    public preSptModLoader: PreSptModLoader;
    public imageRouter: ImageRouter;
    public configServer: ConfigServer;
    public saveServer: SaveServer;
    public itemHelper: ItemHelper;
    public logger: ILogger;
    public staticRouter: StaticRouterModService;
    public fileSystem: FileSystem;
    public hashUtil: HashUtil;
    public inventoryHelper: InventoryHelper;
    public lootGenerator: LootGenerator;
    public eventOutputHolder: EventOutputHolder;

    //initialized at postDBLoad
    public dbTables: IDatabaseTables;
    public dbGlobals: IGlobals;
    public dbItems: Record<string, ITemplateItem>;
    public dbQuests: Record<string, IQuest>;
    public dbTraders: Record<string, ITrader>;
    public dbLocales: ILocaleBase;
    public dbHandbook: IHandbookBase;
    public dbLocations: ILocations;
    public customItem: CustomItemService;
    public jsonUtil: JsonUtil;
    public profileHelper: ProfileHelper;
    public ragfairPriceService: RagfairPriceService;
    public importerUtil: ImporterUtil;
    public botGeneratorHelper: BotGeneratorHelper;

    init(container: DependencyContainer, initStage: InitStage): void {
        if (initStage === InitStage.PRE_SPT_LOAD || initStage === InitStage.ALL) {
            this.container = container;
            this.preSptModLoader = container.resolve<PreSptModLoader>("PreSptModLoader");
            this.imageRouter = container.resolve<ImageRouter>("ImageRouter");
            this.configServer = container.resolve<ConfigServer>("ConfigServer");
            this.saveServer = container.resolve<SaveServer>("SaveServer");
            this.itemHelper = container.resolve<ItemHelper>("ItemHelper");
            this.logger = container.resolve<ILogger>("WinstonLogger");
            this.staticRouter = container.resolve<StaticRouterModService>("StaticRouterModService");
            this.fileSystem = container.resolve<FileSystem>("FileSystem");
            this.hashUtil = container.resolve<HashUtil>("HashUtil");
            this.itemHelper = container.resolve<ItemHelper>("ItemHelper");
            this.inventoryHelper = container.resolve<InventoryHelper>("InventoryHelper");
            this.lootGenerator = container.resolve<LootGenerator>("LootGenerator");
            this.eventOutputHolder = container.resolve<EventOutputHolder>("EventOutputHolder");
        }

        if (initStage === InitStage.POST_DB_LOAD || initStage === InitStage.ALL) {
            this.dbTables = container.resolve<DatabaseServer>("DatabaseServer").getTables();
            this.dbGlobals = this.dbTables.globals;
            this.dbItems = this.dbTables.templates.items;
            this.dbQuests = this.dbTables.templates.quests;
            this.dbTraders = this.dbTables.traders;
            this.dbLocales = this.dbTables.locales;
            this.dbHandbook = this.dbTables.templates.handbook;
            this.dbLocations = this.dbTables.locations;
            this.customItem = container.resolve<CustomItemService>("CustomItemService");
            this.jsonUtil = container.resolve<JsonUtil>("JsonUtil");
            this.profileHelper = container.resolve<ProfileHelper>("ProfileHelper");
            this.ragfairPriceService = container.resolve<RagfairPriceService>("RagfairPriceService");
            this.importerUtil = container.resolve<ImporterUtil>("ImporterUtil");
            this.botGeneratorHelper = container.resolve<BotGeneratorHelper>("BotGeneratorHelper");
        }
    }

    registerStaticRoute(
        routeURL: string,
        routeName: string,
        callable: CallableFunction,
        boundClass: any = undefined, // any call to 'this' in callable will call boundClass
        outputModified: boolean = false
    ): void {
        if (boundClass) {
            callable = callable.bind(boundClass);
        }

        this.staticRouter.registerStaticRouter(
            routeName,
            [
                {
                    url: routeURL,
                    action: async (url: string, info: any, sessionId: string, output: string) => {
                        const Helper = new ModHelper();
                        Helper.init(this.container, InitStage.ALL);

                        const modifiedOutput = callable(url, info, sessionId, output, Helper);

                        if (outputModified) {
                            return modifiedOutput;
                        } else {
                            return output || JSON.stringify({ value_not_needed: true });
                        }
                        return output;
                    },
                },
            ],
            "aki"
        );
    }

    log(logText: string, logColor: LogTextColor = LogTextColor.WHITE, dontLogModName: boolean = false): void {
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

export class MongoMapperSingleton {
    // this class must use the singleton pattern to avoid multiple separate instances being created
    // this is to avoid issues when multiple instances exist and are not in sync with one another
    private static _instance: MongoMapperSingleton;

    private _mappingsPath: string = FileUtils.pathCombine(ModHelper.modPath, "src", "mongo_mappings", "DO_NOT_EDIT.json");
    private _hashUtil: HashUtil;
    private _mongoMap: Record<string, string>;

    private constructor(container: DependencyContainer) {
        this._hashUtil = container.resolve<HashUtil>("HashUtil");

        const mappingsFolderPath = FileUtils.pathCombine(ModHelper.modPath, "src", "mongo_mappings");
        if (!fs.existsSync(mappingsFolderPath)) {
            fs.mkdirSync(mappingsFolderPath);
        }
        if (!fs.existsSync(this._mappingsPath)) {
            this._mongoMap = {};
            FileUtils.writeJson(this._mongoMap, this._mappingsPath);
        }
        this._mongoMap = FileUtils.readJson<Record<string, string>>(this._mappingsPath);
    }

    public static getInstance(container: DependencyContainer): MongoMapperSingleton {
        if (!this._instance) {
            this._instance = new MongoMapperSingleton(container);
        }
        return this._instance;
    }

    public getMongoId(humanReadableId: string): string {
        if (!(humanReadableId in this._mongoMap)) {
            const mongoId = this._hashUtil.generate();
            this._mongoMap[humanReadableId] = mongoId;
            FileUtils.writeJson(this._mongoMap, this._mappingsPath);
            return mongoId;
        }

        return this._mongoMap[humanReadableId];
    }
}
