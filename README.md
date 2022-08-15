# Optimal-delaunay-triangulation-and-building-euclidean-minimum-spanning-tree
![Example of the initial state](https://github.com/Vapvapa/Optimal-delaunay-triangulation-and-building-euclidean-minimum-spanning-tree/blob/master/readme-resources/img0-initial-state.png?raw=true)

In this program, based on the data points, the optimal Delaunay triangulation and the minimum spanning tree are built.

The user can interact with the program using the buttons:
1. "Get points from file"
2. "Generating points"
3. "Delaunay triangulation"
4. "Minimum spanning tree"
5. "Clear"
6. "Save points to file"

## 1. Button "Get points from file"

![Window for opening a file.](https://github.com/Vapvapa/Optimal-delaunay-triangulation-and-building-euclidean-minimum-spanning-tree/blob/master/readme-resources/img1-open-window.png?raw=true)

When you click on the "Get points from file" button, a window is displayed in which you can select a file. The file must be in *.txt format.

![Example of a *.txt file.](https://github.com/Vapvapa/Optimal-delaunay-triangulation-and-building-euclidean-minimum-spanning-tree/blob/master/readme-resources/img2-example-txt.png?raw=true)

The field must contain at least 3 points with x and y coordinates, that is, at least 6 numbers. A dot is used to write a decimal number. Any text can be written in the file, only numbers are recognized. If there are an odd number of numbers, the last number is not saved.

## 2. Button "Generating points"
![Example of generated points.](https://github.com/Vapvapa/Optimal-delaunay-triangulation-and-building-euclidean-minimum-spanning-tree/blob/master/readme-resources/img3-generating-points.png?raw=true)

When you click on the "Point Generation" button, points are generated and drawn.

## 3. Button "Delaunay triangulation"
![Example of constructing an optimal Delaunay triangulation.](https://github.com/Vapvapa/Optimal-delaunay-triangulation-and-building-euclidean-minimum-spanning-tree/blob/master/readme-resources/img4-delaunay-triangulation.png?raw=true)

When you click on the "Delaunay triangulation" button, optimal Delaunay triangulation are generated and drawn (My algorithm is based on the "Delete and build" and "Local rebuilding of triangles" algorithms).

## 4. Button "Minimum spanning tree"
![Example of constructing an minimum spanning tree.](https://github.com/Vapvapa/Optimal-delaunay-triangulation-and-building-euclidean-minimum-spanning-tree/blob/master/readme-resources/img5-minimum-spanning-tree.png?raw=true)

When you click on the "Minimum spanning tree" button, optimal Delaunay triangulation are generated and drawn (Prim's algorithm is used).

## 5. Button "Clear"
![Example of clear.](https://github.com/Vapvapa/Optimal-delaunay-triangulation-and-building-euclidean-minimum-spanning-tree/blob/master/readme-resources/img6-clear.png?raw=true)

When you click on the "Clear" button, the graph will be cleared.

## 6. Button "Save points to file"
![Example of save points to file.](https://github.com/Vapvapa/Optimal-delaunay-triangulation-and-building-euclidean-minimum-spanning-tree/blob/master/readme-resources/img7-save-points.png?raw=true)

When you click on the "Save points to file" button, a window for saving points will appear.