CREATE TABLE "component_types" (
                                   "id"	TEXT NOT NULL UNIQUE,
                                   "type"	TEXT UNIQUE ,
                                   PRIMARY KEY("id")
);

CREATE TABLE "field_data" (
	"id"	INTEGER NOT NULL,
	"component_id"	TEXT,
	"field_name"	INTEGER,
	"is_collection"	INTEGER,
	"collection_index"	INTEGER,
	PRIMARY KEY("id" AUTOINCREMENT)
);

CREATE TABLE "asset_locations" (
    "id" INTEGER NOT NULL,
    "base_gameobject"	TEXT,
    "transform_path"	TEXT,
    "field_id"	INTEGER,
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

INSERT INTO component_types (id, type) VALUES ('-1', 'None');

PRAGMA synchronous = OFF;
PRAGMA journal_mode = MEMORY;