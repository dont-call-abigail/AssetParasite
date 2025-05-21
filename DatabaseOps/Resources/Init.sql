CREATE TABLE "component_types" (
                                   "id"	INTEGER NOT NULL,
                                   "type"	TEXT,
                                   PRIMARY KEY("id" AUTOINCREMENT)
);
CREATE TABLE "script_types" (
                                "id"	INTEGER NOT NULL,
                                "type"	TEXT,
                                PRIMARY KEY("id" AUTOINCREMENT)
);
CREATE TABLE "property_data" (
	"id"	INTEGER NOT NULL,
	"component_id"	INTEGER,
	"script_id"	INTEGER,
	"property_name"	INTEGER,
	"is_collection"	INTEGER,
	"collection_index"	INTEGER,
	PRIMARY KEY("id" AUTOINCREMENT)
);

CREATE TABLE "asset_refs" (
    "id" INTEGER NOT NULL,
    "asset_guid"	TEXT,
    "base_gameobject"	TEXT,
    "transform_path"	TEXT,
    "component_data_id"	INTEGER,
    PRIMARY KEY("id" AUTOINCREMENT)
);

CREATE TABLE "mod_assets" (
                              "instance_id"	INTEGER NOT NULL,
                                "mod_guid" TEXT NOT NULL,
                              "ref_id" INTEGER NOT NULL,
                              PRIMARY KEY("instance_id" AUTOINCREMENT)
);

CREATE TABLE "game_assets" (
                               "instance_id"	INTEGER NOT NULL,
                               "ref_id" INTEGER NOT NULL,
                                PRIMARY KEY("instance_id" AUTOINCREMENT)
);

INSERT INTO script_types (id, type) VALUES (-1, 'None');

PRAGMA synchronous = OFF;
PRAGMA journal_mode = MEMORY;