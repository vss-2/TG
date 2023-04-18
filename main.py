from PIL import Image
pivot = None
size = 64
for imageNumber in range(1,20):
  blackCounter = 0
  with open(f'./bitmap-{imageNumber}.txt','r') as arq:
    pivot = arq.readlines()
  lines = []
  for l in pivot:
    if(l == '\n'):
      continue
    temp = l.split(' ')
    if(temp[-1] == '\n'):
      temp.pop()
    lines.append(temp)
    # lines[len(lines)-1].pop()

  rgb_ints = []
  for l in range(0,int(len(lines)/size)):
    count = 0
    linha = []
    while(count < size):
      if(l == 0 or count == 0):
        if(l == 0):
          # if(len(lines[count]) != 1):
          # print('o'+lines[count]+'i')
          r,g,b = lines[count]
            # linha.append((int(float(r)*255), int(float(g)*255), int(float(b)*255)))
        else: 
          # if(len(lines[l*size]) != 1):
          r,g,b = lines[l*size]
            # linha.append((int(float(r)*255), int(float(g)*255), int(float(b)*255)))
      else:
        # if(len(lines[count+(size*l)]) != 1):
        r,g,b = lines[count+(size*l)]
          # linha.append((int(float(r)*255), int(float(g)*255), int(float(b)*255)))
      # if(r == '0.0' and g == '0.0' and b == '0.0'): 
      #   blackCounter+=1
      linha.append((int(float(r)*255), int(float(g)*255), int(float(b)*255)))
      count+=1
    rgb_ints.append(linha)

  # print(len(rgb_ints[0]), len(rgb_ints))
  # print(blackCounter)
  image = Image.new('RGB', (len(rgb_ints[0]), len(rgb_ints)))
  for y, row in enumerate(rgb_ints):
    for x, color in enumerate(row):
      #print(x,y,color)
      image.putpixel((x, y), color)

  #Save the image to a file
  image.save(f'./image-{imageNumber}.png')
  # with open(f'G:/Unity/SpreadSimulator/New Unity Project/Assets/Scripts/Images/image{imageNumber}.txt', 'w') as arq:
    # pass
  # with open(f'G:/Unity/SpreadSimulator/New Unity Project/Assets/Scripts/Images/image{imageNumber}.txt.meta', 'w') as arq:
    # pass
