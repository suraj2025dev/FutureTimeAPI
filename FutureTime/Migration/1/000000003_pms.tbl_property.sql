
alter table pms.tbl_property add column api_base_url character varying(500) COLLATE pg_catalog."default";
alter table pms.tbl_property add column api_sub_url character varying(50) COLLATE pg_catalog."default";
ALTER TABLE pms.tbl_property RENAME COLUMN url TO host;
alter table pms.tbl_property add column api_key character varying(100) COLLATE pg_catalog."default";
