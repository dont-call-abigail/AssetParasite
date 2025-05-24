CREATE TABLE "component_types" (
                                   "id"	INTEGER NOT NULL,
                                   "type"	TEXT UNIQUE ,
                                   PRIMARY KEY("id" AUTOINCREMENT)
);
CREATE TABLE "script_types" (
                                "id"	INTEGER NOT NULL,
                                "type"	TEXT UNIQUE ,
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

CREATE TABLE "asset_locations" (
    "id" INTEGER NOT NULL,
    "base_gameobject"	TEXT,
    "transform_path"	TEXT,
    "property_id"	INTEGER,
    PRIMARY KEY("id" AUTOINCREMENT)
);

CREATE TABLE "assets" (
                              "instance_id"	INTEGER NOT NULL,
                              "source" TEXT NOT NULL,
                              "asset_guid"	TEXT,
                              "asset_name"    TEXT,
                              "location_id" INTEGER NOT NULL,
                              PRIMARY KEY("instance_id" AUTOINCREMENT)
);

INSERT INTO script_types (id, type) VALUES (-1, 'None');

PRAGMA synchronous = OFF;
PRAGMA journal_mode = MEMORY;