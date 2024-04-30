using Drydock;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TranslatePosition
{
    double scale;
    double rope_scale;
    float offset_x;
    float offset_y;
    private Vector3 rotation_axis = new Vector3(0, 0, 1);

    private GameObject ShipObject;

    private GameObject[] Capstans;
    private GameObject[] Ropes;
    private GameObject[] KeelBlocks;

    private Dock drydock;

    public TranslatePosition(double scale, float offset_x, float offset_y, ref Dock drydock)
    {
        this.scale = scale;
        rope_scale = 7 * scale;
        this.offset_x = offset_x;
        this.offset_y = offset_y;
        this.drydock = drydock;
    }

    public void AddShip(ref GameObject ship)
    {
        ShipObject = ship;
    }
    public void AddCapstans(ref GameObject[] capstans)
    {
        Capstans = capstans;
    }
    public void AddRopes(ref GameObject[] ropes)
    {
        Ropes = ropes;
    }
    public void AddKeelBlocks(ref GameObject[] keel_blocks)
    {
        KeelBlocks = keel_blocks;
    }

    private void Translate(ref myVector input, double input_angle, ref GameObject obj_to_translate)
    {
        // scale and set Vector3 to current position of x (flipped across x-y line for 16:9 monitors)
        float output_x = (float)(input.x * scale) - offset_x;
        float output_y = (float)(input.y * scale) - offset_y;

        Vector3 transform_position = new Vector3();
        transform_position.x = output_x;
        transform_position.y = output_y;

        Quaternion transform_rotation = new Quaternion();
        float angle = (float)input_angle;
        Debug.Log(angle);
        transform_rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, rotation_axis);

        obj_to_translate.transform.SetPositionAndRotation(transform_position, transform_rotation);
    }
    private void TranslateFlipped(ref myVector input, double input_angle, ref GameObject obj_to_translate)
    {
        // scale and set Vector3 to current position of x (flipped across x-y line for 16:9 monitors)
        float output_x = (float)(input.y * scale) - offset_x;
        float output_y = (float)(input.x * scale) - offset_y;

        Vector3 transform_position = new Vector3();
        transform_position.x = output_x;
        transform_position.y = output_y;

        Quaternion transform_rotation = new Quaternion();
        float angle = (float)input_angle;
        transform_rotation = Quaternion.AngleAxis(-180 + (angle * Mathf.Rad2Deg), rotation_axis);

        obj_to_translate.transform.SetPositionAndRotation(transform_position, transform_rotation);
    }

    private void TranslateRopeFlipped(int i)
    {
        Vector3 rope_radius = new Vector3(0, 0, 0);
        Vector3 rope_length = new Vector3(0, 0.2f, 0);
        rope_radius.x = (float)((drydock.capstans[i].line.length * rope_scale) / 2);
        rope_length.x = (float)(drydock.capstans[i].line.length * rope_scale);
        float angle = (float)drydock.capstans[i].line.direction.theta;
        var rope_rotation = Quaternion.AngleAxis(-180 + (angle * Mathf.Rad2Deg), rotation_axis);

        Ropes[i].transform.localScale = rope_length;
        Ropes[i].transform.localPosition = rope_radius;
        Capstans[i].transform.SetPositionAndRotation(Capstans[i].transform.localPosition, rope_rotation);
    }

    public void TranslateAllFlipped()
    {
        TranslateFlipped(ref drydock.ship.pos, drydock.ship.angle + Mathf.PI/2, ref ShipObject);
        for (int i = 0; i < Capstans.Length; i++)
        {
            TranslateFlipped(ref drydock.capstans[i].pos, drydock.capstans[i].line.direction.theta, ref Capstans[i]);
        }
        for (int i = 0; i < Ropes.Length; i++)
        {
            TranslateRopeFlipped(i);
        }
        for (int i = 0; i < KeelBlocks.Length; i++)
        {
            TranslateFlipped(ref drydock.keel_blocks[i].global_position, Mathf.PI / 2, ref KeelBlocks[i]);
        }
    }

    public void TranslateShipFlipped()
    {
        TranslateFlipped(ref drydock.ship.pos, -drydock.ship.angle + Mathf.PI / 2, ref ShipObject);
    }

    public void TranslateRopesFlipped()
    {
        for (int i = 0; i < Ropes.Length; i++)
        {
            TranslateRopeFlipped(i);
        }
    }

}
