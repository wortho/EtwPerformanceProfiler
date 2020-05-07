page 50146 "Performance Profiler Archive"
{
    DeleteAllowed = false;
    InsertAllowed = false;
    ModifyAllowed = false;
    PageType = Worksheet;
    SourceTable = "Performance Profiler Archive";

    layout
    {
        area(content)
        {
            group(Configuration)
            {
                Caption = 'Configuration';
                field("Target Session Code"; TargetSessionCode)
                {
                    Lookup = true;
                    TableRelation = "Performance Session Archived";

                    trigger OnValidate()
                    begin
                        Rec.SetRange("Session Code", TargetSessionCode);
                        CurrPage.Update;
                    end;
                }
                field("Duration Threshold"; DurationThreshold)
                {

                    trigger OnValidate()
                    begin
                        SetFilter(Duration, '%1..', DurationThreshold);
                        CurrPage.Update;
                    end;
                }
                field("Base Duration"; BaseDuration)
                {

                    trigger OnValidate()
                    begin
                        CurrPage.Update;
                    end;
                }
            }
            repeater("Call Tree")
            {
                Editable = false;
                IndentationColumn = Indentation;
                ShowAsTree = true;
                field("Object Type"; "Object Type")
                {
                    Editable = false;
                    Enabled = false;
                }
                field("Object ID"; "Object ID")
                {
                }
                field("Line No"; "Line No")
                {
                }
                field(Statement; Statement)
                {
                }
                field(Duration; Duration)
                {
                    Caption = 'Duration (ms)';
                    Style = Attention;
                }
                field(MinDuration; MinDuration)
                {
                }
                field(MaxDuration; MaxDuration)
                {
                }
                field(LastActive; LastActive)
                {
                }
                field(HitCount; HitCount)
                {
                }
                field(Id; Id)
                {
                }
                field("Session Code"; "Session Code")
                {
                    Visible = false;
                }
                field(Indentation; Indentation)
                {
                }
                field(Ratio; GetRatio)
                {
                    Caption = 'Ratio %';
                    DecimalPlaces = 3 : 3;
                }
            }
        }
    }

    actions
    {
        area(processing)
        {
            action("Calculate Ratio")
            {
                Image = CalculateHierarchy;
                Promoted = true;
                PromotedCategory = Process;
                PromotedIsBig = true;

                trigger OnAction()
                begin
                    BaseDuration := Duration;
                    CurrPage.Update;
                end;
            }
        }
    }

    trigger OnInit()
    begin
        if FindFirst then
            TargetSessionCode := "Session Code";
    end;

    trigger OnOpenPage()
    begin
        Rec.SetRange("Session Code", TargetSessionCode);
    end;

    var
        TargetSessionCode: Code[20];
        BaseDuration: Decimal;
        DurationThreshold: Decimal;

    local procedure GetRatio(): Decimal
    begin
        if BaseDuration <> 0 then
            exit(Duration / BaseDuration * 100);
    end;
}

