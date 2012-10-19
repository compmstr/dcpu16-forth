needs strings.fs
needs util.fs

struct
		\ hash for this cell
		cell% field hashmap-hash
		\ linked list of hashmap-entrys
		cell% field hashmap-items
		\ next hashmap cell
		cell% field hashmap-next
end-struct hashmap

struct
		\ string key
		cell% field hashmap-entry-key
		cell% field hashmap-entry-value
		cell% field hashmap-entry-next
end-struct hashmap-entry

: get-hashmap-cell ( hashmap key -- cell )
		get-counted-string hash-string \ hashmap hash
		swap @ swap \ [hashmap] hash
		begin
				2dup swap \ hashmap hash hash hashmap
				hashmap-hash @ \ hashmap hash hash hashmap-hash
				= not \ hashmap hash (hashmap-hash != hash)
		while \ hashmap hash
						swap hashmap-next @ \ hash [hashmap-next]
						0 over = if
								swap
								leave
						then
		repeat
		\ hashmap hash
		drop
;

: get-hashmap-entry ( hashmap-cell key -- entry )
		get-counted-string \ hashmap-cell len loc
		rot \ len loc hashmap-cell
;

