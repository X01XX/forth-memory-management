
\ For use outside of the GPL 3.0 license,
\ except for stack.fs, mm_array.fs, link.fs, list.fs, tools.fs, stackprint.fs, struct.fs,
\            structlist.fs, structinfo.fs, and structinfolist.fs,
\ contact the Wisconsin Alumni Research Foundation (WARF).

\ The Linux command: alias words='grep "^: "'
\ allows a quick list of functions in a file, like "words session.fs" or more, "words *list.fs"
\ Words can be filtered, like "words list.fs | grep remove" or "words list.fs | grep -- list-set-"
\ A brief comment on the same line as the function name can be helpful.
\ For a bash shell user, the command can be added at the end of ~/.bashrc to make it permanent.
 
\ Struct IDs.
\
\ Each number is prime, 5 digits, and fits within 16 bits.
\ The digit pairs [0][1], [1][2], [2][3], and [3][4] are prime.
\ No digit appears more than twice. (avoids 111)
\ No digit appears consecutively.   (avoids 11)
\
\ Struct ids in use.
\ Link   List   Region Rule   RuleStore Square
\ 17137, 17971, 19317, 23131, 23173,    23197
\
\ Sample Action Session Domain Need   Changes
\ 23719, 29717, 31319,  31379, 19717, 31973
\
\ PlanStep   Plan   Group  Rate   RegionCorrRate  RegionCorr
\ 37171,     37379, 43717, 41719, 41737,          47317
\
\ RuleCorr  Changescorr PathStep Plancorr Corner Stack-info
\ 53171,    53173,      53197,   53717,   53719, 53731
\
\ Struct ids not yet used:
\ 59797, 61379, 61717, 61979.

\ Start a clean vocabulary.
cr ." Starting vocabulary UES," cr
vocabulary UES

\ Put new words into the UES vocabulary.
UES definitions

decimal
\ #2 base !  \ Test all numbers GT 1, LT -1, have a base prefix.

0 value structinfo-list-store   \ Storage for a list containing info on all structs.
                                \ Used for memory use print, memory leak checking,
                                \ freeing heap, struct-aware printing of the Forth stack.

include xtindirect.fs
include bool.fs

include tools.fs
include struct.fs

include mm_array.fs     \ includes stack.fs
include link.fs
include list.fs
include structlist.fs

\ Application.
include globals.fs

include value.fs
include valuelist.fs

include region.fs
include region2.fs

include regionlist.fs

include changes.fs
include changeslist.fs
include changes2.fs
include changescorr.fs

include sample.fs

include rule.fs
include rulestore.fs
include rulelist.fs

include square.fs

include squarelist.fs

include need.fs
include needlist.fs

include corner.fs
include cornerlist.fs

include planstep.fs
include plansteplist.fs

include regioncorr.fs
include regioncorrlist.fs
include rate.fs
include regioncorrrate.fs
include regioncorrratelist.fs
include rulecorr.fs

include plan.fs
include planlist.fs

include plancorr.fs
include plancorrlist.fs

include pathstep.fs
include pathsteplist.fs

include group.fs
include grouplist.fs

include action.fs
include actionlist.fs
include actionxts.fs

include domain.fs
include domainlist.fs

include session.fs

include input.fs

include stackprint.fs
include structinfo.fs
include structinfolist.fs
cs

\ Test files.
include square_t.fs
include corner_t.fs
include squarelist_t.fs
include region_t.fs
include regionlist_t.fs
include rule_t.fs
include action_t.fs
include rulestore_t.fs
include input_t.fs
include regioncorr_t.fs
include regioncorrlist_t.fs
include session_t.fs
include plan_t.fs
include domain_t.fs

cr ." main.fs"

\ Init array-stacks.
#3500 link-mma-init
#1902 list-mma-init
list-new to structinfo-list-store

#0030 structinfo-mma-init
cr ." at 1: " .stack-gbl cr
' link-deallocate ' .link s" Link" link-mma link-id structinfo-new structinfo-list-store structinfo-list-push

' structinfo-list-deallocate-struct-list ' structinfo-list-print-struct-list s" List" list-mma list-id structinfo-new structinfo-list-store structinfo-list-push-end
' structinfo-deallocate ' .structinfo s" StructInfo" structinfo-mma structinfo-id structinfo-new structinfo-list-store structinfo-list-push-end

#3003 region-mma-init
' region-deallocate ' .region s" Region" region-mma region-id structinfo-new structinfo-list-store structinfo-list-push-end

#0600 regioncorr-mma-init
' regioncorr-deallocate ' .regioncorr s" RegionCorr" regioncorr-mma regioncorr-id structinfo-new structinfo-list-store structinfo-list-push-end

#0904 rule-mma-init
' rule-deallocate ' .rule s" Rule" rule-mma rule-id structinfo-new structinfo-list-store structinfo-list-push-end

#0505 rulestore-mma-init
' rulestore-deallocate ' .rulestore s" RuleStore" rulestore-mma rulestore-id structinfo-new structinfo-list-store structinfo-list-push-end

#0404 rulecorr-mma-init
' rulecorr-deallocate ' .rulecorr s" RuleCorr" rulecorr-mma rulecorr-id structinfo-new structinfo-list-store structinfo-list-push-end

#0406 square-mma-init
' square-deallocate ' .square s" Square" square-mma square-id structinfo-new structinfo-list-store structinfo-list-push-end

#0200 corner-mma-init
' corner-deallocate ' .corner s" Corner" corner-mma corner-id structinfo-new structinfo-list-store structinfo-list-push-end

#0250 sample-mma-init
' sample-deallocate ' .sample s" Sample" sample-mma sample-id structinfo-new structinfo-list-store structinfo-list-push-end

#0650 changes-mma-init
' changes-deallocate ' .changes s" Changes" changes-mma changes-id structinfo-new structinfo-list-store structinfo-list-push-end

#0450 changescorr-mma-init
' changescorr-deallocate ' .changescorr s" ChangesCorr" changescorr-mma changescorr-id structinfo-new structinfo-list-store structinfo-list-push-end

#0100 group-mma-init
' group-deallocate ' .group s" Group" group-mma group-id structinfo-new structinfo-list-store structinfo-list-push-end

#0200 need-mma-init
' need-deallocate ' .need s" Need" need-mma need-id structinfo-new structinfo-list-store structinfo-list-push-end

#0150 planstep-mma-init
' planstep-deallocate ' .planstep s" PlanStep" planstep-mma planstep-id structinfo-new structinfo-list-store structinfo-list-push-end

#0370 pathstep-mma-init
' pathstep-deallocate ' .pathstep s" PathStep" pathstep-mma pathstep-id structinfo-new structinfo-list-store structinfo-list-push-end

#0150 plan-mma-init
' plan-deallocate ' .plan s" Plan" plan-mma plan-id structinfo-new structinfo-list-store structinfo-list-push-end

#0040 plancorr-mma-init
' plancorr-deallocate ' .plancorr s" PlanCorr" plancorr-mma plancorr-id structinfo-new structinfo-list-store structinfo-list-push-end

#0100 rate-mma-init
' rate-deallocate ' .rate s" Rate" rate-mma rate-id structinfo-new structinfo-list-store structinfo-list-push-end

#0100 regioncorrrate-mma-init
' regioncorrrate-deallocate ' .regioncorrrate s" RegionCorrRate" regioncorrrate-mma regioncorrrate-id structinfo-new structinfo-list-store structinfo-list-push-end

#0050 action-mma-init
' action-deallocate ' .action s" Action" action-mma action-id structinfo-new structinfo-list-store structinfo-list-push-end

#0025 domain-mma-init
' domain-deallocate ' .domain s" Domain" domain-mma domain-id structinfo-new structinfo-list-store structinfo-list-push-end

#0001 session-mma-init
' session-deallocate ' .session s" Session" session-mma session-id structinfo-new structinfo-list-store structinfo-list-push-end

: init-main ( -- )
    \ Set up session.
    current-session-new                         \ sess, session instance added to session stack.

    \ Add domain 0square_t
    #4 over  domain-new                         \ sess dom

    \ Add actions to domain 0
    [ ' domain-0-act-1-get-sample ] literal     \ sess dom0 xt
    over domain-add-action                      \ sess dom0

    [ ' domain-0-act-2-get-sample ] literal     \ sess dom0 xt
    over domain-add-action                      \ sess dom0

    [ ' domain-0-act-3-get-sample ] literal     \ sess dom0 xt
    over domain-add-action                      \ sess dom0

    [ ' domain-0-act-4-get-sample ] literal     \ sess dom0 xt
    over domain-add-action                      \ sess dom0

    [ ' domain-0-act-5-get-sample ] literal     \ sess dom0 xt
    over domain-add-action                      \ sess dom0

    [ ' domain-0-act-6-get-sample ] literal     \ sess dom0 xt
    over domain-add-action                      \ sess dom0

    \ Add a domain
    over session-add-domain                     \ sess

    \ Add domain 1
    #5 over domain-new                          \ sess dom1

    \ Add actions to domain 1
    [ ' domain-1-act-1-get-sample ] literal     \ sess dom1 xt
    over domain-add-action                      \ sess dom1

    [ ' domain-1-act-2-get-sample ] literal     \ sess dom1 xt
    over domain-add-action                      \ sess dom1

    [ ' domain-1-act-3-get-sample ] literal     \ sess dom1 xt
    over domain-add-action                      \ sess dom1

    [ ' domain-1-act-4-get-sample ] literal     \ sess dom1 xt
    over domain-add-action                      \ sess dom1

    [ ' domain-1-act-5-get-sample ] literal     \ sess dom1 xt
    over domain-add-action                      \ sess dom1

    [ ' domain-1-act-6-get-sample ] literal     \ sess dom1 xt
    over domain-add-action                      \ sess dom1

    \ Add last domain
    over session-add-domain                     \ sess dom1

    s" (X1X1 01X1X)" regioncorr-from-string-a   \ sess regc
    -1 #2 rate-new                              \ sess regc rt
    regioncorrrate-new                          \ sess regc-rt
    over session-add-regioncorrrate             \ sess

    s" (1XX1 01X1X)" regioncorr-from-string-a   \ sess regc
    #-2 0 rate-new                              \ sess
    regioncorrrate-new                          \ sess regc-rt
    over session-add-regioncorrrate             \ sess

    s" (00X1 11XXX)" regioncorr-from-string-a   \ sess regc
    0 #2 rate-new                               \ sess
    regioncorrrate-new                          \ sess regc-rt
    over session-add-regioncorrrate             \ sess

    .session
;

0 value step-num
: main ( -- )
    init-main
    0 to step-num
    true
    begin
    while
        \ Inc step num.

        step-num 1+ to step-num

        \ Print header.
        cr ." ***************************"
        cr ." Step: " step-num .
        space ." Current state: "
        current-session dup .session-current-states     \ sess
        space ." Reachable "
        dup .session-reachable-regions                  \ sess
        space

        dup session-get-current-rate                    \ sess rate

        space ." rate: "
        dup .rate                                       \ sess rate
        space ." Status: "
        dup rate-get-positive
        0= if
            \ No positive value.
            dup rate-get-negative
            0= if
                \ No negative value.
                ." Neutral"
            else
                 \ Some negative value.
                ." Negative"
            then
        else
            \ Some positive value.
            dup rate-get-negative
            0= if
                \ No negative value.
                ." Positive"
            else
                \ Some negative value.
                ." Conflicted"
            then
        then
        rate-deallocate                                 \ sess
        drop                                            \
        cr

        #80 s" Enter command: > " get-user-input
        \ cr .s cr
        depth 1 <>
        if
            ." depth not equal one? " .s
            abort
        then
    repeat

    \ Clean up

    \ Print memory use before deallocating.
    cr structinfo-list-store structinfo-list-print-memory-use

    cr ." Deallocating ..." cr
    current-session-deallocate

    \ Print memory use after deallocating.
    cr structinfo-list-store structinfo-list-print-memory-use

    \ Check for memory leak, or something on the Forth stack.
    structinfo-list-store structinfo-list-project-deallocated-xt execute

    \ Free heap.
    \ structinfo-list-store structinfo-list-free-heap
;

: all-tests
    structinfo-list-store structinfo-list-project-deallocated-xt execute

    square-tests
    square-list-tests
    corner-tests
    region-tests
    region-list-tests
    rule-tests
    action-tests
    rulestore-tests
    input-tests
    regioncorr-tests
    regioncorr-list-tests
    session-tests
    plan-tests
    domain-tests

    structinfo-list-store structinfo-list-project-deallocated-xt execute
;
