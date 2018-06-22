--[beginscript]

create table CK.tCKTrait
(
	CKTraitId int not null identity(0,1),
	CKTraitContextId int not null,
	AtomicWordId int not null,
	TraitName varchar(4096) collate LATIN1_General_100_BIN2 not null,

	constraint PK_CK_CKTrait primary key nonclustered (CKTraitId),
	constraint FK_CK_CKTrait_CKTraitContextId foreign key (CKTraitContextId) references CK.tCKTraitContext(CKTraitContextId),
	constraint FK_CK_CKTrait_AtomicWordId foreign key (AtomicWordId) references CK.tCKTraitWord(CKTraitWordId)
);
create unique clustered index IX_CK_CKTrait_CKTraitContextId on CK.tCKTrait (CKTraitContextId,CKTraitId);
create unique index IX_CK_CKTrait_AtomicWordId on CK.tCKTrait (CKTraitContextId,AtomicWordId) where AtomicWordId <> 0;

-- We don't create an empty trait for each context. 
-- Only the one of the 0 context exists and its identifier is 0. 
insert into CK.tCKTrait( CKTraitContextId, AtomicWordId, TraitName ) 
	values( 0, 0, '' );

--[endscript]
