#include<SDL/SDL.h>
#include<SDL/SDL_image.h>
#include<stdio.h>
#include<string.h>

int main(int argc, char **argv){
	SDL_Init(SDL_INIT_VIDEO);
  IMG_Init(IMG_INIT_PNG);
  SDL_Surface *img = IMG_Load("font.png");

  // font image is 128x32, or 32x4 chars
  if(img->w != 128 || img->h != 32 || img->format->BytesPerPixel != 3){
    printf("Invalid image\nNeeds to be 128x32 with 24 bits per pixel\n");
		return -1;
  }

	int buffer_size = 32 * 4;
	unsigned int buffer[buffer_size];
	memset(buffer, 0, buffer_size * sizeof(unsigned int));
	unsigned int buffer_loc = 0;

	unsigned int bpp = img->format->BytesPerPixel;
	int base_x = 0;
	int base_y = 0;
	int cur_x = 0;
	int cur_y = 0;
	Uint8 * pixels = img->pixels;
	Uint8 pix;
	for(int y = 0; y < 4; y++){
		base_y = y * 8;
		for(int x = 0; x < 32; x++){
			base_x = x * 4;
			for(int bit = 0; bit < 32; bit++){
				cur_x = base_x + (bit / 8);
				cur_y = base_y + (7 - (bit % 8));
				pix = pixels[(cur_x * bpp)
										 + (cur_y * img->pitch)];
				if(pix >= 128){
					pix = 1;
				}else{
					pix = 0;
				}
				buffer[buffer_loc] |= (pix << (31 - bit));
			}
			buffer_loc++;
		}
	}

	FILE *fout = fopen("font.dfnt", "w");
	fwrite((Uint8 *)buffer, sizeof(unsigned int), buffer_size, fout);
	fclose(fout);

  IMG_Quit();
	SDL_Quit();
  return 0;
}
