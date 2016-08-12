row=150;  col=210;
fin=fopen('C:\Users\malik\OneDrive\Documents\Visual Studio 2015\Projects\FaceTracking\FaceTracking\29140\d[00-16-536].raw','r');
I=fread(fin,row*col,'uint8=>uint8'); 
Z=reshape(I,row,col);
Z=Z';
figure, imshow(Z)
%imwrite(Z, 'grey.png')