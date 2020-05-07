table 50145 "Performance Profiler Events"
{
    DataPerCompany = false;

    fields
    {
        field(1; Id; Integer)
        {
        }
        field(2; "Session ID"; Integer)
        {
        }
        field(3; Indentation; Integer)
        {
        }
        field(4; "Object Type"; Option)
        {
            Caption = 'Object Type';
            OptionCaption = 'TableData,Table,Form,Report,Dataport,Codeunit,XMLport,MenuSuite,Page,Query,System,FieldNumber';
            OptionMembers = TableData,"Table",Form,"Report",Dataport,"Codeunit","XMLport",MenuSuite,"Page","Query",System,FieldNumber;
        }
        field(5; "Object ID"; Integer)
        {
            Caption = 'Object ID';
            TableRelation = Object.ID WHERE(Type = FIELD("Object Type"));
            //This property is currently not supported
            //TestTableRelation = false;
        }
        field(6; "Line No"; Integer)
        {
        }
        field(7; Statement; Text[250])
        {
        }
        field(8; Duration; Decimal)
        {
        }
        field(9; MinDuration; Decimal)
        {
        }
        field(10; MaxDuration; Decimal)
        {
        }
        field(11; LastActive; Decimal)
        {
        }
        field(12; HitCount; Integer)
        {
        }
        field(13; Total; Decimal)
        {
            CalcFormula = Sum ("Performance Profiler Events".Duration WHERE(Indentation = CONST(0)));
            FieldClass = FlowField;
        }
        field(14; FullStatement; BLOB)
        {
        }
    }

    keys
    {
        key(Key1; Id, "Session ID")
        {
            Clustered = true;
        }
        key(Key2; Indentation)
        {
        }
    }

    fieldgroups
    {
    }

    var
        DeleteArchiveQst: Label 'Existing lines of the archived session %1 will be deleted. Do you want to continue?';
        CopyedToArchMsg: Label 'Data is archived with Session Code %1.';

    [Scope('Internal')]
    procedure CopyToArchive(CurrSessionId: Integer)
    var
        ProfilerArchive: Record "Performance Profiler Archive";
        ProfilerEvent: Record "Performance Profiler Events";
        SessionCode: Code[20];
    begin
        if FindSessionCode(SessionCode) then begin
            ProfilerEvent.Reset;
            ProfilerEvent.SetRange("Session ID", CurrSessionId);
            if ProfilerEvent.FindSet then begin
                repeat
                    ProfilerArchive.Init;
                    ProfilerArchive.TransferFields(ProfilerEvent);
                    ProfilerArchive."Session Code" := SessionCode;
                    ProfilerArchive.Insert;
                until ProfilerEvent.Next = 0;
                Message(CopyedToArchMsg, SessionCode);
            end;
        end;
    end;

    local procedure FindSessionCode(var SessionCode: Code[20]): Boolean
    var
        ProfilerSessionArch: Record "Performance Session Archived";
        PerfSessionArchived: Page "Performance Session Archived";
    begin
        PerfSessionArchived.LookupMode(true);
        if PerfSessionArchived.RunModal = ACTION::LookupOK then begin
            PerfSessionArchived.GetRecord(ProfilerSessionArch);
            ProfilerSessionArch.TestField(Code);
            SessionCode := ProfilerSessionArch.Code;

            exit(RemoveExistingArchLines(SessionCode));
        end;
        exit(false);
    end;

    local procedure RemoveExistingArchLines(SessionCode: Code[20]): Boolean
    var
        ProfilerArchive: Record "Performance Profiler Archive";
    begin
        ProfilerArchive.Reset;
        ProfilerArchive.SetRange("Session Code", SessionCode);
        if not ProfilerArchive.IsEmpty then begin
            if not Confirm(DeleteArchiveQst, false, SessionCode) then
                exit(false);
            ProfilerArchive.DeleteAll;
        end;
        exit(true);
    end;
}

