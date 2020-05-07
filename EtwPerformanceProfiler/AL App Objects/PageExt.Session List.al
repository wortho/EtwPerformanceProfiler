pageextension 50145 SessionListPerformanceProfiler extends "Session List"
{
    actions
    {
        addafter("Debug Next Session")
        {
            action("Performance Profiler")
            {
                Caption = 'Performance Profiler';
                Image = Capacity;
                Promoted = true;
                PromotedCategory = Category4;
                PromotedIsBig = true;

                trigger OnAction()
                var
                    PerfProfiler: Page "Performance Profiler";
                begin
                    if "Session ID" > 0 then
                        PerfProfiler.SetTargetSessionID("Session ID");

                    PerfProfiler.RUN;
                end;
            }
            action("Terminate Session")
            {
                Caption = 'Terminate Session';
                Image = Delete;
                Promoted = true;
                PromotedCategory = Category4;
                PromotedIsBig = true;
                Ellipsis = true;

                trigger OnAction()
                var
                    TerminateConfirmTxt: Label 'Terminate selected session?';
                begin
                    if Confirm(TerminateConfirmTxt, true) then
                        StopSession("Session ID");
                end;
            }
        }
    }
}