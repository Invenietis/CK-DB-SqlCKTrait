--[beginscript]

create table CK.tCKTraitSet
(
	CKTraitId int not null,
	CKTraitWordId int not null,

	constraint PK_CK_CKTraitSet primary key (CKTraitId,CKTraitWordId),
	constraint FK_CK_CKTraitSet_CKTraitId foreign key (CKTraitId) references CK.tCKTrait(CKTraitId),
	constraint FK_CK_CKTraitSet_CKTraitWordId foreign key (CKTraitWordId) references CK.tCKTraitWord(CKTraitWordId)
);
create unique index IX_CK_CKTraitSet on CK.tCKTraitSet(CKTraitWordId,CKTraitId);

-- Binds the Empty trait singleton (the CKTraitId 0) to the empty word.
insert into CK.tCKTraitSet( CKTraitId, CKTraitWordId ) 
	values( 0, 0 );

--[endscript]
