table 50147 "Performance Session Archived"
{
    DataCaptionFields = "Code", Description;
    LookupPageID = "Performance Session Archived";

    fields
    {
        field(1; "Code"; Code[20])
        {
        }
        field(2; Description; Text[250])
        {
        }
    }

    keys
    {
        key(Key1; "Code")
        {
            Clustered = true;
        }
    }

    fieldgroups
    {
    }

    trigger OnDelete()
    var
        PerformanceProfilerArchive: Record "Performance Profiler Archive";
    begin
        PerformanceProfilerArchive.SetRange("Session Code", Code);
        PerformanceProfilerArchive.DeleteAll;
    end;
}

