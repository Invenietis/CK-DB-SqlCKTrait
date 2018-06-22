--[beginscript]

create table CK.tCKTraitWord
(
	CKTraitWordId int not null identity(0,1),
	Word varchar(128) collate LATIN1_General_100_BIN2 not null, 

	constraint PK_CK_CKTraitWord primary key (CKTraitWordId)
);
create unique index IX_CK_CKTraitWord_Word on CK.tCKTraitWord (Word,CKTraitWordId);

-- The empty word.
insert into CK.tCKTraitWord( Word ) values( '' );

--[endscript]
