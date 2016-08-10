using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public class Points : MonoBehaviour
{
    Vector3 FAvoid, vec;
    float a, b, c, discr, dist, mag, min, newZ, tau, t;
    int i, j, min_idx, ref_index;
    GameObject[] agent;             // array of people
    Vector3[] gv;                   // array of goal velocities
    Vector3[] F;                    // array of forces
    Vector3[] pos;                  // array of agent positions
    Vector3[] velocity;             // array of agent velocities
    Vector3[] line1, line2, line3, line4, line5;
    bool[] collision;
    List<int>[] nni;                // array of nearest neighbors
    Vector3[] refr;                 // array of reference points
    int[][] nn;                     // array of nearest neighbors   
    int[,] idx;                     // index of agent location
    int[] ref_idx;
    float[] newY;                   // array of new Y positions
    float d = .45F;                 // diameter of person
    float k = 1;                    // force constant
    float r = 14;                   // radius of podium
    float tH = 4;                   // latest time to collision
    float maxF = 3;                // max force between people
    float distPodium;
    float inf = 9000000;            // arbitrarily represents infinity
    float dt = .075F;               // time step
    float timePerAgent = .025F;      // number of seconds simulation runs per agent
    int numAgents = 0;              // number of agents
    int numReference = 6562;        // number of reference grid points
    bool UpdateOn = true;           // continue simulation while true
    bool on = true;
    Vector3 podium = new Vector3(.3355F, 0F, -.9424F);      // location of podium
    Vector3 stop = new Vector3(0, 0, 0);                    // velocity of stopped agent
    string start_points = "C:/Users/malik/OneDrive/Documents/MATLAB/pnyx_rand_points.txt";
    string reference_points = "C:/Users/malik/OneDrive/Documents/MATLAB/pnyx_reference_points.txt";
    string crowd_sim_points = "C:/Users/malik/OneDrive/Documents/Pnyx/pnyx_crowd_simulation.txt";

    // Initialize each point created in Matlab as an agent
    void Start()
    {
        StreamReader point_count = new StreamReader(start_points);
        StreamReader points = new StreamReader(start_points);
        StreamReader references = new StreamReader(reference_points);

        //Parse file for number of agents
        while (!point_count.EndOfStream) {
            point_count.ReadLine();
            numAgents++;
        }

        //Intialize gameobject and position for agents
        agent = new GameObject[numAgents];
        pos = new Vector3[numAgents];
        velocity = new Vector3[numAgents];

        //Parse file to assign positions to each gameobject
        while (!points.EndOfStream)
        {
            // Read in all points from txt file
            string point = points.ReadLine();
            float x = System.Single.Parse(point.Split(',')[0]);
            float y = System.Single.Parse(point.Split(',')[1]);
            float z = System.Single.Parse(point.Split(',')[2]);

            // Create agent for each x,y,z point
            agent[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // Scale agent to size of human
            agent[i].transform.localScale = new Vector3(d, 1.75F, d);
            agent[i].transform.position = pos[i] = new Vector3(x, y + 0.875F, z);
            // Add velocity component to each agent
            velocity[i] = new Vector3(0, 0, 0);
            //Disable colliders to increase performance
            agent[i].GetComponent<Collider>().enabled = false;
            // Color each agent randomly
            agent[i].GetComponent<Renderer>().material.color = new
                Color(UnityEngine.Random.Range(0, 255)/255F, UnityEngine.Random.Range(0, 255) / 255F, UnityEngine.Random.Range(0, 255) / 255F);
            i++;
        }

        i = 0;
        j = 0;
        refr = new Vector3[numReference];

        //Parse file for positions of reference points
        while (!references.EndOfStream)
        {
            // Read in all points from txt file
            string reference = references.ReadLine();
            float xr = System.Single.Parse(reference.Split(',')[0]);
            float yr = System.Single.Parse(reference.Split(',')[1]);
            float zr = System.Single.Parse(reference.Split(',')[2]);
            refr[i] = new Vector3(xr, yr, zr);
            i++;
        }

        // Initialize arrays
        gv = new Vector3[numAgents];
        F = new Vector3[numAgents];
        nn = new int[numAgents][];
        idx = new int[numAgents, 6];
        newY = new float[numAgents];
        nni = new List<int>[numReference];
        collision = new bool[numAgents];
        for (i = 0; i < numReference; i++) { nni[i] = new List<int>(); }
        line1 = new Vector3[] { new Vector3(9.759F, 0, 10.6F), new Vector3(20F, 0, 40F) } ;
        line2 = new Vector3[] { new Vector3(-9.759F, 0, 10.6F), new Vector3(-20F, 0, 40F) } ;
        line3 = new Vector3[] { new Vector3(15F, 0, 5F), new Vector3(50F, 0, 30F) };
        line4 = new Vector3[] { new Vector3(-15F, 0, 5F), new Vector3(-50F, 0, 30F) };
        line5 = new Vector3[] { new Vector3(0, 0, 15F), new Vector3(0, 0, 60F) };

        // Stop simulation after time has elapsed
        //Invoke("updateOff", timePerAgent*numAgents);
    }

    // Update the position of each agent dependent on goal and avoidance forces
    void Update()
    {
        //Update runs within time lapse
        if (UpdateOn == true)
        {
            on = false;
            //Find reference points and calculate Y position each update
            for (int j = 0; j < numAgents; j++)
            {
                nni[idx[j, 0]].Remove(j);
                newY[j] = reference(j);
                collision[j] = false;
            }

            //Adds nearest neignbors to each gameobject based on reference points
            for (int j = 0; j < numAgents; j++)
            {
                if (nni[idx[j, 0]].Count < 15)
                {
                    nn[j] = new int[nni[idx[j, 0]].Count + nni[idx[j, 1]].Count + nni[idx[j, 2]].Count
                        + nni[idx[j, 4]].Count + nni[idx[j, 5]].Count];
                    nni[idx[j, 0]].CopyTo(nn[j], 0);
                    nni[idx[j, 1]].CopyTo(nn[j], nni[idx[j, 0]].Count);
                    nni[idx[j, 2]].CopyTo(nn[j], nni[idx[j, 0]].Count + nni[idx[j, 1]].Count);
                    nni[idx[j, 4]].CopyTo(nn[j], nni[idx[j, 0]].Count + nni[idx[j, 1]].Count + nni[idx[j, 2]].Count);
                    nni[idx[j, 5]].CopyTo(nn[j], nni[idx[j, 0]].Count + nni[idx[j, 1]].Count + nni[idx[j, 2]].Count + nni[idx[j, 4]].Count);
                }
                else {
                    nn[j] = new int[15];
                    nni[idx[j, 0]].RemoveRange(15, nni[idx[j, 0]].Count - 15);
                    nni[idx[j, 0]].CopyTo(nn[j], 0);
                }

            }

            // Calculate direction and avoidance force for each agent
            for (int j = 0; j < numAgents; j++)
            {
                // Calculate direction and magnitude of goal velocity
                gv[j] = podium - pos[j];
                gv[j].Normalize();
                distPodium = Vector3.Distance(podium, pos[j]);
                if (distPodium > 50) gv[j] = gv[j] * .5F;
                else gv[j] = gv[j] * .5F * (Vector3.Distance(podium, pos[j]) / 50);
                // Calculate force
                F[j] = k * (gv[j] - velocity[j]);

                // Calculate avoidance force
                for (i = 0; i < nn[j].Length; i++)
                {
                    if (pos[nn[j][i]] != pos[j])
                    {
                        // Find time-to-collision for each near neighbor
                        t = ttc(j, nn[j][i]);

                        double dis = Vector3.Distance(pos[nn[j][i]], pos[j]);
                        // Apply avoidance force if ttc or position is close
                        if (t < inf || dis <= .45F)
                        {
                            // Some mathematics
                            FAvoid = pos[j] + velocity[j] * t
                                - pos[nn[j][i]] - velocity[nn[j][i]] * t;
                            if (FAvoid[0] != 0 && FAvoid[2] != 0) FAvoid /= Mathf.Sqrt(Vector3.Dot(FAvoid, FAvoid));

                            // Apply magnitude to avoidance force based on ttc
                            mag = 0;
                            if (t > 0 && t <= tH) mag = (tH - t) / (t + .001F);
                            // Maximamize magnitude to avoid skewed avoidance forces
                            if (dis > .45) { if (mag > maxF) mag = maxF; }
                            else { if (mag > 14) mag = 14; }
                            if (Vector3.Distance(pos[nn[j][i]], pos[j]) <= .45F) mag = 3;
                            FAvoid *= mag;
                            // Only add avoidance force to x,z positions
                            F[j][0] += FAvoid[0];
                            F[j][2] += FAvoid[2];
                        }
                        if (dis < .5) collision[j] = true;
                    }
                }
            }

            // Add forces to velocity and position of agents
            for (int j = 0; j < numAgents; j++)
            {
                if (pos[j][0] > 0 && !collision[j])
                {
                    ClosestPointOnLine(j, line1);
                    //ClosestPointOnLine(j, line3);
                }
                else if (!collision[j])
                {
                    //ClosestPointOnLine(j, line2);
                    //ClosestPointOnLine(j, line4);
                }
                ClosestPointOnLine(j,line5);

                // If agent is at goal or has more than threshold of nn remove its velocity
                if ((pos[j][2] <= r + .5F && pos[j][0] >= -r && pos[j][0] <= r && velocity[j][0] != 0) || (nni[idx[j, 0]].Count + nni[idx[j, 1]].Count + nni[idx[j, 2]].Count
                        + nni[idx[j, 4]].Count + nni[idx[j, 5]].Count) > 25)
                {
                    velocity[j] = stop;
                }
                // Only move agents if behind podium
                else if ((pos[j][2] > r + .5F || pos[j][0] <= -r || pos[j][0] >= r))
                {
                    velocity[j] += F[j] * dt;
                    pos[j] += velocity[j] * dt;
                    agent[j].transform.position = pos[j];

                    // Assigns new Y position 
                    if (newY[j] != pos[j][1])
                    {
                        pos[j] = new Vector3(pos[j][0], (pos[j][1] + newY[j]) / 2 + .875F, pos[j][2]);
                        if (pos[j][1] == float.NaN) pos[j][0] = 2;
                        agent[j].transform.position = pos[j];
                    }

                    // Very magical numbers to keep agents within bounds of Pnyx
                    if (((41.13) * (pos[j][2] - 6.24) - (-8.908) * (pos[j][0] + 54.32)) < 0)
                    {
                        newZ = Convert.ToSingle((-.216) * pos[j][0] - 5.49);
                        pos[j] = new Vector3(pos[j][0], pos[j][1], newZ);
                        agent[j].transform.position = pos[j];
                    }
                    if (((-40.23) * (pos[j][2] - 5) - (-8.005) * (pos[j][0] - 53.17)) > 0)
                    {
                        newZ = Convert.ToSingle((.199) * pos[j][0] - 5.58);
                        pos[j] = new Vector3(pos[j][0], pos[j][1], newZ);
                        agent[j].transform.position = pos[j];
                    }
                    if (pos[j][0] < -32.5) on = true;
                }
            }
            if (on == false) UpdateOn = false;
        }
    }

    // Calculate and return time-to-collision constant
    private float ttc(int i, int j)
    {
        // A lot of mathematics/physics
        Vector3 w = pos[j] - pos[i];
        Vector2 w1 = new Vector2(w[0], w[2]);
        c = Vector2.Dot(w1, w1) - (d * d);
        if (c < 0) return 0;
        vec = velocity[i] - velocity[j];
        a = Vector3.Dot(vec, vec);
        b = Vector3.Dot(w, vec);
        discr = b * b - a * c;
        if (discr <= 0) return inf;
        tau = (b - Mathf.Sqrt(discr)) / a;
        if (tau < 0) return inf;
        return tau;
    }

    // Stop simulation after certain time and writes positions to text file
    void updateOff()
    {
        UpdateOn = false;
        using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@crowd_sim_points, true))
        {
            file.Flush();
            for (int j = 0; j < agent.Length; j++)
            {
                file.WriteLine(pos[j][0].ToString() + ',' + pos[j][1].ToString() + ',' + pos[j][2].ToString());
            }
        }
    }

    // Finds six nearest reference points locations
    private float reference(int m)
    {
        min = inf;
        ref_idx = new int[9];

        // Searches for small range of reference points where gameobject is near
        if (pos[m][2] >= 30) {
            if (pos[m][2] >= 60) { min_idx = 5670; }
            else if (pos[m][2] >= 58) { min_idx = 5508; }
            else if (pos[m][2] >= 56) { min_idx = 5346; }
            else if (pos[m][2] >= 54) { min_idx = 5184; }
            else if (pos[m][2] >= 52) { min_idx = 5022; }
            else if (pos[m][2] >= 50) { min_idx = 4860; }
            else if (pos[m][2] >= 48) { min_idx = 4698; }
            else if (pos[m][2] >= 46) { min_idx = 4536; }
            else if (pos[m][2] >= 44) { min_idx = 4374; }
            else if (pos[m][2] >= 42) { min_idx = 4212; }
            else if (pos[m][2] >= 40) { min_idx = 4050; }
            else if (pos[m][2] >= 38) { min_idx = 3888; }
            else if (pos[m][2] >= 36) { min_idx = 3726; }
            else if (pos[m][2] >= 34) { min_idx = 3564; }
            else if (pos[m][2] >= 32) { min_idx = 3402; }
            else { min_idx = 3240; }
        }
        else { 
            if (pos[m][2] >= 28) { min_idx = 3078; }
            else if (pos[m][2] >= 26) { min_idx = 2916; }
            else if (pos[m][2] >= 24) { min_idx = 2754; }
            else if (pos[m][2] >= 22) { min_idx = 2592; }
            else if (pos[m][2] >= 20) { min_idx = 2430; }
            else if (pos[m][2] >= 18) { min_idx = 2268; }
            else if (pos[m][2] >= 16) { min_idx = 2106; }
            else if (pos[m][2] >= 14) { min_idx = 1944; }
            else if (pos[m][2] >= 12) { min_idx = 1782; }
            else if (pos[m][2] >= 10) { min_idx = 1620; }
            else if (pos[m][2] >= 8) { min_idx = 1458; }
            else if (pos[m][2] >= 6) { min_idx = 1296; }
            else if (pos[m][2] >= 4) { min_idx = 1134; }
            else if (pos[m][2] >= 2) { min_idx = 972; }
            else if (pos[m][2] >= 0) { min_idx = 810; }
            else if (pos[m][2] >= -2) { min_idx = 648; }
            else { min_idx = 567; }
        }

        if (pos[m][0] >= 0)
        {
            if (pos[m][0] >= 56.875) { ref_index = 76; }
            else if (pos[m][0] >= 53.625) { ref_index = 74; }
            else if (pos[m][0] >= 50.375) { ref_index = 72; }
            else if (pos[m][0] >= 47.125) { ref_index = 70; }
            else if (pos[m][0] >= 43.875) { ref_index = 68; }
            else if (pos[m][0] >= 40.625) { ref_index = 66; }
            else if (pos[m][0] >= 37.375) { ref_index = 64; }
            else if (pos[m][0] >= 34.125) { ref_index = 62; }
            else if (pos[m][0] >= 30.875) { ref_index = 60; }
            else if (pos[m][0] >= 27.625) { ref_index = 58; }
            else if (pos[m][0] >= 24.375) { ref_index = 56; }
            else if (pos[m][0] >= 21.125) { ref_index = 54; }
            else if (pos[m][0] >= 17.875) { ref_index = 52; }
            else if (pos[m][0] >= 14.625) { ref_index = 50; }
            else if (pos[m][0] >= 11.375) { ref_index = 48; }
            else if (pos[m][0] >= 8.125) { ref_index = 46; }
            else if (pos[m][0] >= 4.875) { ref_index = 44; }
            else if (pos[m][0] >= 1.625) { ref_index = 42; }
            else { ref_index = 41; }
        }
        else {
            if (pos[m][0] >= -3.25) { ref_index = 39; }
            else if (pos[m][0] >= -6.5) { ref_index = 37; }
            else if (pos[m][0] >= -9.75) { ref_index = 35; }
            else if (pos[m][0] >= -13) { ref_index = 33; }
            else if (pos[m][0] >= -16.25) { ref_index = 31; }
            else if (pos[m][0] >= -19.5) { ref_index = 29; }
            else if (pos[m][0] >= -22.75) { ref_index = 27; }
            else if (pos[m][0] >= -26) { ref_index = 25; }
            else if (pos[m][0] >= -29.25) { ref_index = 23; }
            else if (pos[m][0] >= -32.5) { ref_index = 21; }
            else if (pos[m][0] >= -35.75) { ref_index = 19; }
            else if (pos[m][0] >= -39) { ref_index = 17; }
            else if (pos[m][0] >= -42.25) { ref_index = 15; }
            else if (pos[m][0] >= -45.5) { ref_index = 13; }
            else if (pos[m][0] >= -48.75) { ref_index = 11; }
            else if (pos[m][0] >= -52) { ref_index = 9; }
            else if (pos[m][0] >= -55.25) { ref_index = 7; }
            else { ref_index = 5; }
        }

        ref_idx[0] = min_idx + ref_index - 1;
        ref_idx[1] = ref_idx[0] + 1;
        ref_idx[2] = ref_idx[1] + 1;
        ref_idx[3] = ref_idx[0] + 81;
        ref_idx[4] = ref_idx[1] + 81;
        ref_idx[5] = ref_idx[2] + 81;
        ref_idx[6] = ref_idx[0] + 162;
        ref_idx[7] = ref_idx[1] + 162;
        ref_idx[8] = ref_idx[2] + 162;

        // Finds nearest reference point
        for (i = 0; i < 9; i++)
        {
            if (refr[ref_idx[i]][0] != 1000)
            {
                dist = Vector3.Distance(pos[m], refr[ref_idx[i]]);
                if (dist < min)
                {
                    idx[m, 0] = ref_idx[i];
                    min = dist;
                }
            }
        }

        // Checks for edge cases and second nearest reference point
        if (refr[idx[m, 0]][2] == refr[(idx[m, 0] + 1)][2]) idx[m, 1] = idx[m, 0] + 1;
        else
        {
            idx[m, 1] = idx[m, 0];
            idx[m, 0] = idx[m, 0] - 1;
        }

        // Calculates last four reference points
        if (min_idx <= 1620) {
            idx[m, 2] = idx[m, 0] + 81;
            idx[m, 3] = idx[m, 1] + 81;
            idx[m, 4] = idx[m, 0] - 1;
            idx[m, 5] = idx[m, 0] - 81;
            nni[idx[m, 0]].Add(m);
        }
        else {
            idx[m, 2] = idx[m, 0] - 81;
            idx[m, 3] = idx[m, 1] - 81;
            idx[m, 4] = idx[m, 0] - 1;
            idx[m, 5] = idx[m, 0] + 81;
            nni[idx[m, 0]].Add(m);
        }

        // Interpolate to calculate and return Y position 
        try
        {
            float y1 = refr[idx[m, 0]][1] + ((pos[m][0] - refr[idx[m, 0]][0]) / (refr[idx[m, 1]][0] - refr[idx[m, 0]][0])) * (refr[idx[m, 1]][1] - refr[idx[m, 0]][1]);
            float y2 = refr[idx[m, 2]][1] + ((pos[m][0] - refr[idx[m, 2]][0]) / (refr[idx[m, 3]][0] - refr[idx[m, 2]][0])) * (refr[idx[m, 3]][1] - refr[idx[m, 2]][1]);
            float y = y1 + ((pos[m][2] - refr[idx[m, 0]][2]) / (refr[idx[m, 2]][2] - refr[idx[m, 0]][2])) * (y2 - y1);
            return y;
        }
        catch (IndexOutOfRangeException e) {
            return 2;
        }
    }

    private void ClosestPointOnLine(int j, Vector3[] line)
    {
        Vector3 vA = line[0];
        Vector3 vB = line[1];
        Vector3 point = new Vector3(pos[j][0], 0, pos[j][2]);
        Vector3 vClosestPoint;
        double m = Math.Abs((vB[0] - vA[0]) * (vA[2] - point[2]) - (vA[0] - point[0]) * (vB[2] - vA[2])) /
            Math.Sqrt(Math.Pow(vB[0] - vA[0], 2) + Math.Pow(vB[2] - vA[2], 2));
        if (m < .5)
        {
            var vVector1 = point - vA;
            var vVector2 = (vB - vA).normalized;

            var d = 31.1329;
            var t = Vector3.Dot(vVector2, vVector1);

            if (t <= 0) {
                vClosestPoint = vA;
            }
            else if (t >= d)
            {
                vClosestPoint = vB;
            }

            else {
                var vVector3 = vVector2 * t;
                vClosestPoint = vA + vVector3;
            }
            point = Vector3.MoveTowards(pos[j], vClosestPoint, -5F * (60 / Vector3.Distance(podium, pos[j])));
            if (Vector3.Distance(pos[j], podium) > 50)
            {
                if (vA[0] == line1[0][0]) { pos[j][2] = pos[j][2] - Math.Abs(point[2] - pos[j][2]) * .05F; }
                if (vA[0] == line2[0][0]) { pos[j][2] = pos[j][2] - Math.Abs(point[2] - pos[j][2]) * .05F; }
                if (vA[0] == line3[0][0]) { pos[j][0] = pos[j][0] - Math.Abs(point[0] - pos[j][0]) * .05F; }
                if (vA[0] == line4[0][0]) { pos[j][0] = pos[j][0] + Math.Abs(point[0] - pos[j][0]) * .05F; }
            }
            else {
                pos[j][0] = (point[0] + pos[j][0]) / 2;
                pos[j][2] = (point[2] + pos[j][2]) / 2;
            }
        }
    }
}
