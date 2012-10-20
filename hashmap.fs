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

: hashmap-add-entry-to-cell ( entry cell -- )
		\ set the next of the new entry to the first of the cell's entries
		dup hashmap-items @ \ entry cell first-item
		2 pick \ entry cell first-item entry
		hashmap-entry-next ! \ entry cell
		hashmap-items !
;

: new-hashmap-entry ( key value -- hashmap-entry )
		hashmap-entry %allocate throw \ key value loc
		dup rot swap \ key loc value loc
		hashmap-entry-value ! \ key loc
		dup -rot \ loc key loc
		hashmap-entry-key ! \ loc
		0 over hashmap-entry-next ! \ loc
;

: new-hashmap-cell ( key value -- hashmap-cell )
		over get-counted-string hash-string \ key value hash
		hashmap %allocate throw \ key value hash loc
		dup -rot \ key value loc hash loc
		hashmap-hash ! \ key value loc
		-rot \ loc key value
		\ just setting first item, don't have to worry abou add-entry-to-cell
		new-hashmap-entry \ loc entry
		over hashmap-items ! \ loc
		0 over hashmap-next !
;

\ takes pointer to hashmap and counted string key
: get-hashmap-cell ( hashmap key -- cell )
		get-counted-string hash-string \ hashmap hash
		begin
				2dup swap \ hashmap hash hash hashmap
				hashmap-hash @ \ hashmap hash hash hashmap-hash
				= not \ hashmap hash (hashmap-hash != hash)
		while \ hashmap hash
						swap hashmap-next @ \ hash [hashmap-next]
						0 over = if
								swap drop
								exit
						then
		repeat
		\ hashmap hash
		drop
;

: get-hashmap-entry ( hashmap-cell key -- entry )
		get-counted-string \ hashmap-cell len loc
		rot \ len loc hashmap-cell
;

