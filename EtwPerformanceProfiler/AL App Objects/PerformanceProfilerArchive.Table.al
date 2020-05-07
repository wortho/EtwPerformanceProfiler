table 50146 "Performance Profiler Archive"
{
    DataPerCompany = false;

    fields
    {
        field(1; Id; Integer)
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
        field(20; "Session Code"; Code[20])
        {
        }
    }

    keys
    {
        key(Key1; "Session Code", Id)
        {
            Clustered = true;
        }
    }

    fieldgroups
    {
    }
}

