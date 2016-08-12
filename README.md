# Pnyx
Matlab and Unity are the main programs I use to simulate crowds in the Pnyx
# The steps I took are outlined here:
1. Interpolation;
 Using "scaled_floor_60.csv" I had a reference to 350 X,Y,Z positions for the Pnyx
* "Pnyx_crowd_interpolation_refined.m" handles the reference points from the csv and interpolates the data using a Matlab function called scatteredInterpolant
* The rest of the file plots some test points and creates an obj file, but it is not used
2. Spatial Partition
	a.	"pnyx_index.m" is ran after interpolation to create an 81x81 grid of Y reference points
	b.	Writes to "pnyx_reference_points.txt" which is used in Unity for Y lookup
3. Random Position Generator
	a.	"rand_points.m" is ran after interpoolation to create n number of random points in the Pnyx
			A.	n = 1200 would simulate 1000 points that are spaced apart while n = 10000 only produces 5500
			B.	So if you are looking to simulated 10000s of people you will have to increase radius to ~70 m
	c.	Writes to "pnyx_rand_points.txt" which is used in Unity for intializing game objects
4. Crowd Simulation
	a.	"Points.cs" is the main Unity file for simulation. Everything should be commented in the file
			A.	Writes to "pnyx_crowd_simulation.txt" after the simulation completes
	b.	"FPS.cs" is Unity function for displaying fps during simulation
* "grey_byte_to_image.m" is Matlab function for coverting depth byte array to image for face tracking program
