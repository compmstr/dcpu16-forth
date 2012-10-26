needs shorts.fs
needs util.fs

struct
		\ one of the LOC_... constants
		short% field vmloc-type
		\ Register to use
		short% field vmloc-register
		\ Ram location to use
		short% field vmloc-loc
		\ Value for literals
		short% field vmloc-val
end-struct vmloc

\ interrupt queue entry
struct
		short% field intq-entry-message
		cell% field intq-entry-next
		cell% field intq-entry-prev
end-struct intq-entry

