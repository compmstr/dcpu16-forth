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

