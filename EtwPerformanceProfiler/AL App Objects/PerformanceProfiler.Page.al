page 50145 "Performance Profiler"
{
    DeleteAllowed = false;
    InsertAllowed = false;
    ModifyAllowed = true;
    PageType = List;
    PromotedActionCategories = 'New,Process,Report,Archive';
    SourceTable = "Performance Profiler Events";
    UsageCategory = Administration;
    ApplicationArea = All;
    LinksAllowed = false;

    layout
    {
        area(content)
        {
            group(Configuration)
            {
                Caption = 'Configuration';
                field("Target Session ID"; TargetSessionID)
                {
                    Editable = TargetSessionIDEditable;
                    Lookup = true;
                    TableRelation = "Active Session"."Session ID";

                    trigger OnValidate()
                    begin
                        if (TargetSessionID <> MultipleSessionsId) then
                            Rec.SetFilter("Session ID", '=%1', TargetSessionID);
                    end;
                }
                field(Threshold; Threshold)
                {
                }
                field(ProfileMultipleSessions; ProfileMultipleSessions)
                {
                    Caption = 'Profile multiple Sessions';

                    trigger OnValidate()
                    begin
                        if (ProfileMultipleSessions) then begin
                            TargetSessionID := MultipleSessionsId;
                            TargetSessionIDEditable := false
                        end else begin
                            TargetSessionID := SessionId();
                            TargetSessionIDEditable := true
                        end;
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
                field("Session ID"; "Session ID")
                {
                }
            }
            group(Control17)
            {
                ShowCaption = false;
                field(Total; Total)
                {
                    Caption = 'Total Time (ms):';
                    Editable = false;
                    Enabled = false;
                    Style = Strong;
                    StyleExpr = TRUE;
                }
            }
        }
    }

    actions
    {
        area(processing)
        {
            action(Start)
            {
                Enabled = NOT ProfilerStarted;
                Image = Start;
                Promoted = true;
                PromotedCategory = Process;
                PromotedIsBig = true;
                PromotedOnly = true;

                trigger OnAction()
                begin
                    Clear(Rec);

                    if (MultipleSessionsId = TargetSessionID) then begin
                        Clear(Rec);
                        Rec.Init;
                        Rec.DeleteAll;
                    end else begin
                        Rec.SetFilter("Session ID", '=%1', TargetSessionID);
                        Rec.DeleteAll;
                    end;

                    PerformanceProfiler.Start(TargetSessionID, Threshold);

                    ProfilerStarted := true;
                end;
            }
            action(Stop)
            {
                Enabled = ProfilerStarted;
                Image = Stop;
                Promoted = true;
                PromotedCategory = Process;
                PromotedIsBig = true;
                PromotedOnly = true;

                trigger OnAction()
                begin
                    PerformanceProfiler.Stop;
                    ProfilerStarted := false;

                    WaitForDataToBeCollected;

                    CopyEventsFromProfilerToTable;
                    if (TargetSessionID <> MultipleSessionsId) then
                        Rec.SetFilter("Session ID", '=%1', TargetSessionID);
                end;
            }
            action("Analyze ETL File")
            {
                Enabled = NOT ProfilerStarted;
                Image = AnalysisView;
                Promoted = true;
                PromotedCategory = Process;
                PromotedIsBig = true;
                PromotedOnly = true;

                trigger OnAction()
                var
                    FileManagement: Codeunit "File Management";
                    ETLFileName: Text;
                begin
                    ETLFileName := FileManagement.OpenFileDialog('Analyze ETL File', '', 'Trace Files (*.etl)|*.etl');

                    if (ETLFileName <> '') then begin
                        PerformanceProfiler.AnalyzeETLFile(ETLFileName, Threshold);

                        Clear(Rec);
                        Rec.Init;
                        Rec.DeleteAll;

                        CopyEventsFromProfilerToTable;
                    end;
                end;
            }
            action("Clear Codeunit 1 calls")
            {
                Image = ClearFilter;
                Promoted = true;
                PromotedCategory = Process;
                PromotedIsBig = true;
                PromotedOnly = true;

                trigger OnAction()
                var
                    codeUnit1Call: Boolean;
                begin
                    codeUnit1Call := false;

                    FindFirst;

                    repeat
                        if (Indentation = 0) then begin
                            if (("Object Type" = "Object Type"::Codeunit) and ("Object ID" = 1)) then
                                codeUnit1Call := true
                            else
                                codeUnit1Call := false;
                        end;

                        if (codeUnit1Call) then
                            Delete;
                    until Next = 0;
                end;
            }
            action("Get Statement")
            {
                Image = Comment;
                Promoted = true;
                PromotedCategory = Process;
                PromotedIsBig = true;
                PromotedOnly = true;

                trigger OnAction()
                var
                    InStream: InStream;
                    StatementBigTxt: BigText;
                    StatementTxt: Text;
                begin
                    CalcFields(FullStatement);

                    FullStatement.CreateInStream(InStream);
                    StatementBigTxt.Read(InStream);

                    StatementBigTxt.GetSubText(StatementTxt, 1, StatementBigTxt.Length);

                    Message(StatementTxt);
                end;
            }
            action("Copy To Archive")
            {
                Image = CopyWorksheet;
                Promoted = true;
                PromotedCategory = Category4;
                PromotedIsBig = true;
                PromotedOnly = true;

                trigger OnAction()
                begin
                    CopyToArchive("Session ID");
                end;
            }
            action(Archive)
            {
                Image = Archive;
                Promoted = true;
                PromotedCategory = Category4;
                PromotedIsBig = true;
                PromotedOnly = true;
                RunObject = Page "Performance Profiler Archive";
            }
        }
    }

    trigger OnAfterGetCurrRecord()
    begin
        CalcFields(Total);
    end;

    trigger OnInit()
    begin
        MaxStatementLength := 250;
        Threshold := 0;
        TargetSessionID := SessionId();
        TargetSessionIDEditable := true;
        MultipleSessionsId := -1;
    end;

    trigger OnOpenPage()
    begin
        // Assume that profiler hasn't been started.
        // This assumption might be wrong.
        ProfilerStarted := false;
        if (TargetSessionID <> MultipleSessionsId) then
            Rec.SetFilter("Session ID", '=%1', TargetSessionID);

        if (Rec.IsEmpty) then begin
            Rec."Session ID" := TargetSessionID;
            Rec.Insert;
        end;

        PerformanceProfiler := PerformanceProfiler.EtwPerformanceProfiler();
    end;

    var
        ProgressDialog: Dialog;
        PerformanceProfiler: DotNet EtwPerformanceProfiler;
        [InDataSet]
        ProfilerStarted: Boolean;
        TargetSessionIDEditable: Boolean;
        [InDataSet]
        TargetSessionID: Integer;
        PleaseWaitCollectingDataTxt: Label 'Collecting performance data \\Please wait #1';
        Threshold: Integer;
        MultipleSessionsId: Integer;
        MaxStatementLength: Integer;
        ProfileMultipleSessions: Boolean;

    local procedure WaitForDataToBeCollected()
    var
        SecondsToWait: Integer;
    begin
        SecondsToWait := 3;
        ProgressDialog.Open(PleaseWaitCollectingDataTxt);
        while SecondsToWait > 0 do begin
            ProgressDialog.Update(1, StrSubstNo('%1 s', SecondsToWait));
            Sleep(1000);
            SecondsToWait -= 1;
        end;
        ProgressDialog.Close;
    end;

    local procedure CopyEventsFromProfilerToTable()
    var
        OutStream: OutStream;
        I: Integer;
        StatementTxt: Text;
        StatementBigTxt: BigText;
    begin
        I := 1;

        while (PerformanceProfiler.CallTreeMoveNext) do begin
            Clear(Rec);
            Rec.Init;
            Rec.Id := I;
            Rec."Session ID" := PerformanceProfiler.CallTreeCurrentStatementSessionId;
            Rec.Indentation := PerformanceProfiler.CallTreeCurrentStatementIndentation;
            Rec."Object Type" := PerformanceProfiler.CallTreeCurrentStatementOwningObjectType;
            Rec."Object ID" := PerformanceProfiler.CallTreeCurrentStatementOwningObjectId;
            Rec."Line No" := PerformanceProfiler.CallTreeCurrentStatementLineNo;

            StatementTxt := PerformanceProfiler.CallTreeCurrentStatement;
            if (StrLen(StatementTxt) > MaxStatementLength) then begin
                Statement := CopyStr(StatementTxt, 1, MaxStatementLength);
            end else begin
                Rec.Statement := StatementTxt;
            end;
            Clear(StatementBigTxt);
            StatementBigTxt.AddText(StatementTxt);
            FullStatement.CreateOutStream(OutStream);
            StatementBigTxt.Write(OutStream);

            Rec.Duration := PerformanceProfiler.CallTreeCurrentStatementDurationMs;
            Rec.MinDuration := PerformanceProfiler.CallTreeCurrentStatementMinDurationMs;
            Rec.MaxDuration := PerformanceProfiler.CallTreeCurrentStatementMaxDurationMs;
            Rec.LastActive := PerformanceProfiler.CallTreeCurrentStatementLastActiveMs;
            Rec.HitCount := PerformanceProfiler.CallTreeCurrentStatementHitCount;
            Rec.Insert;

            I += 1;
        end;
    end;

    [Scope('Internal')]
    procedure SetTargetSessionID(NewTargetSessionID: Integer)
    begin
        // SO
        TargetSessionID := NewTargetSessionID;
    end;
}

