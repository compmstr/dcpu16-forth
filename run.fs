hex
: print-argc
  argc @ . ." Arguments"
;
print-argc
include vm/vm.fs run-test-file -e bye
