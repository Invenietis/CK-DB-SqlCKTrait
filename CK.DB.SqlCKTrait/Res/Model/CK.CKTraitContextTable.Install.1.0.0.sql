--[beginscript]

create table CK.tCKTraitContext
(
	CKTraitContextId int not null identity(0,1),
	ContextName varchar(128) collate LATIN1_General_100_BIN2 not null,
	Separator char collate LATIN1_General_100_BIN2 not null,
	constraint PK_CK_CKTraitContext primary key (CKTraitContextId),
  constraint UK_CK_CKTraitContext_ContextName unique( ContextName )
);

-- The context 0 can not be used to stay in sync with the C# implementation where
-- a CKTraitContext has necessarily a non empty name.
insert into CK.tCKTraitContext( ContextName, Separator ) values( '', ' ' );

-- The context 1 is a reserved System context.
insert into CK.tCKTraitContext( ContextName, Separator ) values( 'System', '|' );

--[endscript]
