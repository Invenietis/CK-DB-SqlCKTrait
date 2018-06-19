-- SetupConfig: { "Requires": "CK.sCKTraitDestroy" }
--
--   Removes a trait by its full name.
--   The same normalization as in sCKTraitFindOrCreate applies and the same
--   limitation regarding destroying atomic traits as in sCKTraitDestroy.
-- 
alter procedure CK.sCKTraitDestroyByTraitName
(
	@ActorId int,
	@CKTraitContextId int,
	@TraitName varchar(4096)
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @TraitName is null throw 50000, 'Argument.MustNotBeNull', 1;
    if @CKTraitContextId is null or @CKTraitContextId <= 0 throw 50000, 'Argument.InvalidCKTraitContextId', 1;

    --[beginsp]
	declare @Separator char;
	select @Separator = Separator from CK.tCKTraitContext where CKTraitContextId = @CKTraitContextId;
	if @Separator is null 
	begin
		;throw 50000, 'CKTrait.InvalidCKTraitContextId', 1;
	end

	declare @WordTable table ( Word varchar(128) collate LATIN1_General_100_BIN2 not null );
	declare @NbWords int;
	insert into @WordTable
		select distinct ltrim(rtrim(value)) as W 
				from string_split( @TraitName, @Separator ) 
				where len(ltrim(rtrim(value))) > 0;
	set @NbWords = @@ROWCOUNT;
	if @NbWords > 0
	begin
		declare @DestroyedTraitId int;
		select @DestroyedTraitId = t.CKTraitId 
			from CK.tCKTraitSet s
			inner join CK.tCKTrait t on t.CKTraitId = s.CKTraitId and t.CKTraitContextId = @CKTraitContextId
			where not exists
			( 
			  (
				(select tW.CKTraitWordId from CK.tCKTraitWord tW inner join @WordTable W on W.Word = tW.Word)
					except
				(select CKTraitWordId from CK.tCKTraitSet x where x.CKTraitId = s.CKTraitId)
			  )
			  union all
			  (
				(select CKTraitWordId from CK.tCKTraitSet x where x.CKTraitId = s.CKTraitId)
					except
				(select tW.CKTraitWordId from CK.tCKTraitWord tW inner join @WordTable W on W.Word = tW.Word)
		      )
			);
		if @DestroyedTraitId is not null
		begin
			exec CK.sCKTraitDestroy @ActorId, @DestroyedTraitId;
		end
	end
	--[endsp]
end
