
include tools.fs
include struct.fs
include mm_array.fs
include link.fs
include list.fs
include structlist.fs
include numlist.fs
include name.fs
include namelist.fs

clearstack
\ ### M A I N ####
#100 link-mma-init       \ Initialize store for linked list links. link-mma is set.
 #15 list-mma-init       \ Initialize linked list header store. list-mma is set.
 #50 name-mma-init       \ Initialize name struct array.  name-mma is set.

\ Print memory use for mm-arays.
: memory-use ( -- )
    cr
    ." Memory Use:"
    cr #4 spaces ." list" space
    list-mma .mma-usage
    cr #4 spaces ." link" space
    link-mma .mma-usage
    cr #4 spaces ." name" space
    name-mma .mma-usage
    cr #4 spaces ." dstack: "
    base @ >r decimal .s r> base !
;

cr memory-use cr

list-new value list1   \ Get linked list header for a new list, store it in word list1

\ Here, a number will be stored in the data field in a link, instead of a struct instance address.
\ ************************************************************************************************

cr ." *** Working with lists of numbers, list link data field is a number." cr

#5 list1 list-push
#3 list1 list-push
#1 list1 list-push
#6 list1 list-push
#5 list1 list-push

\ To avoid duplicates, create num-list-push, which will use list-member.

cr
." list1: "
' . list1 .list

list-new value list2   \ Get linked list header for a new list, store it in word list2
#1 list2 list-push
#2 list2 list-push
#5 list2 list-push
#5 list2 list-push
#3 list2 list-push
cr
." list2: "
' . list2 .list
cr
." list1 list2 intersection: "
' = list1 list2 list-intersection value list3
' . list3 .list
cr
." list1 list2 union: "
' = list1 list2 list-union value list4
' . list4 .list

cr
." list1 - list2: "
' = list1 list2 list-difference value list5
' . list5 .list

cr
." list2 - list1: "
' = list2 list1 list-difference value list6
' . list6 .list
cr
." list7: "
' * #2 list1 num-list-apply value list7
list7 .num-list
#3 spaces ." (list1 * 2)"

cr
." list8: "
' + -1 list1 num-list-apply value list8
list8 .num-list
#3 spaces ." (list1 + -1)"

cr memory-use cr

cr
." Deallocating num-lists ..."
cr

list1 num-list-deallocate
list2 num-list-deallocate
list3 num-list-deallocate
list4 num-list-deallocate
list5 num-list-deallocate
list6 num-list-deallocate
list7 num-list-deallocate
list8 num-list-deallocate

cr memory-use

\ Here, a struct instance address will be stored in the data field in a link.
\
\ ***************************************************************************
cr
cr ." *** Working with lists of Name struct instances, list link data field is an instance address."
cr ." *** Name struct instances keep track of use count, see namelist.fs, name-deallocate in name.fs."
cr
list-new value list9   \ Start a new linked list.
s" Mary" name-new list9 list-push-struct
s" Dan" name-new list9 list-push-struct
s" Dave" name-new list9 list-push-struct
s" Cindy" name-new list9 list-push-struct
cr ." list9: " ' .name list9 .list

cr
list-new value list10   \ Start a new linked list.
s" Mary" name-new list10 list-push-struct
s" Dan" name-new list10 list-push-struct
s" Dave" name-new list10 list-push-struct
s" Max" name-new list10 list-push-struct
cr ." list10: " ' .name list10 .list

' name-eq list9 list10 list-intersection-struct value list11
cr cr ." list9 list10 intersection: " ' .name list11 .list

' name-eq list9 list10 list-union-struct value list12
cr cr ." list9 list10 union: " ' .name list12 .list

' name-eq list9 list10 list-difference-struct value list13
cr cr ." list9 - list10: " ' .name list13 .list

' name-eq list10 list9 list-difference-struct value list14
cr cr ." list10 - list9: " ' .name list14 .list

cr memory-use

cr cr ." Deallocating name lists ..."

list9  name-list-deallocate
list10 name-list-deallocate
list11 name-list-deallocate
list12 name-list-deallocate
list13 name-list-deallocate
list14 name-list-deallocate
cr memory-use

assert-list-mma-none-in-use
assert-link-mma-none-in-use
assert-name-mma-none-in-use
assert-forth-stack-empty

\ Free heap memory.
cr
cr ." Freeing heap memory"
list-mma mma-free
link-mma mma-free
name-mma mma-free
cr

