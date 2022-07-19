import yaml
from PIL import Image

with open("lightning2.png.meta", "r", encoding="utf-8") as f:
    data = yaml.safe_load(f)

img = Image.open("lightning2.png").convert('RGBA')
xsize, ysize = img.size
pixels = img.load()

for spr in data["TextureImporter"]["spriteSheet"]["sprites"]:
    r = spr["rect"]
    img_export = Image.new('RGBA', (r["width"], r["height"]))
    pixels_export = img_export.load()
    for x in range(r["width"]):
        for y in range(r["height"]):
            pixels_export[x, y] = pixels[r["x"] + x, ysize - r["height"] - r["y"] + y]
    img_export.save(f"{spr['name']}.png")


