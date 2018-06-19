-- SetupConfig: {}
-- 
-- Finds or creates (optionaly only finds) a trait based on a full string of 
-- traits separated by the configured separator of the provided @CKTraitContextId.
-- The traits are normalized (trimmed, sorted, deduplicated): this reproduces 
-- the C# implementation of the CKTrait.
--
alter procedure CK.sCKTraitFindOrCreate 
	@ActorId int,
	@CKTraitContextId int,
	@FindOnly bit,
	@TraitName varchar(4096),
	@CKTraitIdResult int output
as
begin
	if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
	if @TraitName is null throw 50000, 'Argument.MustNotBeNull', 1;
	if @CKTraitContextId is null or @CKTraitContextId <= 0 throw 50000, 'Argument.InvalidCKTraitContextId', 1;
	set @CKTraitIdResult = null;

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
	if @NbWords = 0
	begin
		-- If there is no traits, this is the empty trait.
		set @CKTraitIdResult = 0;
	end
	else
	begin
		-- Inserts or updates the words, capturing their identifiers.
		declare @IdTable table ( WordId int not null, Word varchar(128) collate LATIN1_General_100_BIN2 not null );
		merge CK.tCKTraitWord as target
			using (select Word from @WordTable) as source on target.Word = source.Word
			when not matched then
				insert (Word) values (source.Word)
			when matched then 
				-- This fake update enables the line to appear (as a $action='UPDATE'). 
				update set Word = source.Word
			output inserted.CKTraitWordId, source.Word into @IdTable;

		-- If there is only one word, lookup for the atomic tait.
		declare @MustLookupTraits bit = 1;
		if @NbWords = 1 
		begin
			set @MustLookupTraits = 0;
			select @CKTraitIdResult = t.CKTraitId
				from CK.tCKTrait t
				inner join @IdTable w on w.WordId = t.AtomicWordId
				where t.CKTraitContextId = @CKTraitContextId;
		end
		if @CKTraitIdResult is null and @FindOnly = 0
		begin
			-- Creates all atomic traits that do not currently exist in the context.
			declare @WordId int;
			declare @AtomicTraitName varchar(128);
			declare @CWord cursor;
			set @CWord = cursor local fast_forward for 
				select WordId, Word 
					from @IdTable 
					where WordId not in (select AtomicWordId from CK.tCKTrait where CKTraitContextId = @CKTraitContextId and AtomicWordId <> 0);
			open @CWord;
			fetch from @CWord into @WordId, @AtomicTraitName;
			while @@FETCH_STATUS = 0
			begin
				set @MustLookupTraits = 0;

				--<PreAtomicCreate />

				insert into CK.tCKTrait( CKTraitContextId, AtomicWordId, TraitName ) 
					  values ( @CKTraitContextId, @WordId, @AtomicTraitName );
				-- Captures the last created atomic trait in @CKTraitIdResult.
				set @CKTraitIdResult = scope_identity();
				insert into CK.tCKTraitSet( CKTraitId, CKTraitWordId )
					  values ( @CKTraitIdResult, @WordId );

				--<PostAtomicCreate revert /> 

				fetch next from @CWord into @WordId, @AtomicTraitName;
			end
			deallocate @CWord;
			-- If only one atomic trait has been created, we are done.
			-- Otherwise we must create the composite one.
			if @NbWords <> 1 set @CKTraitIdResult = null;
		end
		-- If any atomic trait has been created or if it was only an atomic trait, 
		-- it is useless to lookup for an existing trait.
		if @MustLookupTraits = 1
		begin
			-- Lookup of the potential existing trait.
			select @CKTraitIdResult = t.CKTraitId 
				from CK.tCKTraitSet s
				inner join CK.tCKTrait t on t.CKTraitId = s.CKTraitId and t.CKTraitContextId = @CKTraitContextId
				where not exists
				( 
				  ((select WordId from @IdTable) except (select CKTraitWordId from CK.tCKTraitSet x where x.CKTraitId = s.CKTraitId))
					  union all
				  ((select CKTraitWordId from CK.tCKTraitSet x where x.CKTraitId = s.CKTraitId) except (select WordId from @IdTable))
				);
		end
		if @CKTraitIdResult is null and @FindOnly = 0
		begin
			--<PreCreate />
			insert into CK.tCKTrait( CKTraitContextId, AtomicWordId, TraitName ) 
				values 
				(
					@CKTraitContextId,
					0,
					Stuff((select @Separator + Word from @WordTable order by Word for xml path(''),TYPE).value('text()[1]','varchar(4096)'),1,1,N'')
				 );
			set @CKTraitIdResult = scope_identity();
			insert into CK.tCKTraitSet( CKTraitId, CKTraitWordId )
				select @CKTraitIdResult, WordId from @IdTable;
			--<PostCreate revert /> 
		end
	end

	-- We can only have a null @CKTraitIdResult if @FindOnly is 1: this
	-- should be an Assert.
	if @FindOnly = 1 and @CKTraitIdResult is null set @CKTraitIdResult = 0;

	--[endsp]
end
