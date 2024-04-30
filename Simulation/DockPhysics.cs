using System;
using static System.Math;
using System.IO;
using Newtonsoft.Json.Converters;
using UnityEngine;
using Unity.VisualScripting;

namespace Drydock
{
    [Serializable]
    public class myVector
    {
        public double x;
        public double y;
        public double r;
        public double theta;
        public myVector(double _x, double _y)
        {
            x = _x;
            y = _y;
            r = Sqrt(Pow(x, 2) + Pow(y, 2));
            if (_y == 0)
            {
                theta = 0;
            }
            else
            {
                theta = Atan(_x / _y);
            }
        }

        public myVector(double _x, double _y, double _r)
        {
            x = _x;
            y = _y;
            r = _r;
        }

        public myVector(myVector newVector)
        {
            x = newVector.x;
            y = newVector.y;
            r = Sqrt(Pow(x, 2) + Pow(y, 2));
            // converts x and y to angle from positive x-axis
            update_theta();
        }

        public void update_theta()
        {
            theta = Atan2(x, y);
            r = Sqrt(Pow(x, 2) + Pow(y, 2));
        }
        public void equals(double _x, double _y)
        {
            // set myVector components to some values
            x = _x;
            y = _y;
        }
        public double total()
        {
            // compute hypotenuse of x and y (not updated every frame)
            r = Sqrt(Pow(x, 2) + Pow(y, 2));
            return r;
        }
        public void add(myVector v)
        {
            // add v to this vector
            x += v.x;
            y += v.y;
        }
        public myVector subtract(myVector v)
        {
            return new myVector(x - v.x, y - v.y);
        }

        public static myVector operator +(myVector v1, myVector v2)
            => new myVector(v1.x + v2.x, v1.y + v2.y);

        public static myVector operator -(myVector a, myVector b)
            => new myVector(a.x - b.x, a.y - b.y);
        public myVector multiply(double d)
        {
            return new myVector(x * d, y * d);
        }
        public double dot(myVector v)
        {
            // returns dot product of this vector and v
            return (x * v.x) + (y * v.y);
        }
        public double cross(myVector v)
        {
            // returns cross product of this vector and v
            return (x * v.y) - (y * v.x);
        }
        public double project(myVector v)
        {
            // returns projection of v onto this vector
            if (r != 0)
            {
                return dot(v) / r;
            }
            else
            {
                return 0;
            }
        }

        public void rotate(double alpha)
        {
            // rotates x and y around origin by alpha
            double newAngle = theta - alpha;
            y = r * Cos(newAngle);
            x = r * Sin(newAngle);
        }
    }

    [Serializable]
    public class Rope
    {
        public double E;
        public double A;
        public double AE;
        public double tension;
        public double avg_strength = 628500; // N
        public double slope1;
        public double slope2;

        public double length;
        public double slack = 0;
        public double stretch = 0;

        public myVector direction = new myVector(0, 0, 1); //UNIT myVector
        public myVector attach_point = new myVector(0, 0);

        public Rope(double _E, double d)
        {
            E = _E;
            A = 0.25 * PI * Pow(d, 2);
            AE = A * E;
            slack = 0;
            stretch = 0;
            slope1 = (0.1 * avg_strength) / 0.015;
            slope2 = (0.15 * avg_strength) / 0.015;
        }

        public Rope(Rope newRope)
        {
            E = newRope.E;
            A = newRope.A;
            AE = A * E;
            tension = 0;
            avg_strength = newRope.avg_strength;
            slope1 = newRope.slope1;
            slope2 = newRope.slope2;
            length = 0;
            slack = 0;
            stretch = 0;
            direction = new myVector(newRope.direction);
            direction.update_theta();
            attach_point = new myVector(newRope.attach_point);
        }

        public void settings_setup()
        {
            // set values in preparation to convert Dock object to JSON
            stretch = 0;
            slack = 0;
            tension = 0;
        }

        public void update_tension()
        {
            // piecewise stretch vs tension to match spectra 12-strand graph
            //if (stretch < 0.1)
            //{
                tension = slope1 * stretch / length;
            //}
            //else
            //{
                //tension = slope2 * stretch / length;
            //}
        }
        public void update_stretch(double tension)
        {
            // sets rope stretch to the stretch produced by input "tension"
            stretch = (tension * length) / slope1;
            //if (stretch > 0.1)
            //{
                //stretch = (tension * length) / slope2;
            //}
        }
        
        public void update_direction(myVector capstan_pos, myVector ship_pos, double ship_angle)
        {
            // update direction rope pulls from
            attach_point.rotate(ship_angle);
            double x = capstan_pos.x - (ship_pos.x + attach_point.x);
            double y = capstan_pos.y - (ship_pos.y + attach_point.y);

            length = Sqrt(Pow(x, 2) + Pow(y, 2));

            direction.equals(x / length, y / length);
            direction.r = 1;
            direction.update_theta();
        }
    }

    [Serializable]
    public class Capstan
    {
        public int n;
        public double coeff_friction_k = 0.6;
        public double coeff_friction_s = 0.7;
        public double F_stall;
        public double F_brake;
        public bool slip = false;
        public bool brake = false;

        public double n_turns = 0;
        public double T_hold = 0;
        public double set_speed = 0;
        public double speed = 0;
        public double slow_speed = 0;
        public double fast_speed = 0;
        public double light_hold = 0;
        public double strong_hold = 0;
        private double stored_speed = 0;

        public myVector pos = new myVector(0, 0);
        public myVector force = new myVector(0, 0);
        public double F_MAX;

        public Rope line = new Rope(0, 0);

        public Capstan(int _n)
        {
            n = _n;
            F_stall = 60000;
            F_brake = 70000;
            slow_speed = 0;
            fast_speed = 0;
            stored_speed = slow_speed; // setup capstan so that on/off button will switch it to slow speed
        }

        public Capstan(Capstan newCapstan)
        {
            n = newCapstan.n;
            coeff_friction_k = newCapstan.coeff_friction_k;
            coeff_friction_s = newCapstan .coeff_friction_s;
            F_stall = newCapstan.F_stall;
            F_brake = newCapstan.F_brake;
            slip = false;
            brake = false;
            n_turns = 0;
            T_hold = 0;
            set_speed = 0;
            speed = 0;
            slow_speed = newCapstan.slow_speed;
            fast_speed = newCapstan.fast_speed;
            stored_speed = slow_speed;
            light_hold = newCapstan.light_hold;
            strong_hold = newCapstan.strong_hold;
            pos = new myVector(newCapstan.pos);
            force = new myVector(newCapstan.force);
            F_MAX = newCapstan.F_MAX;
            line = new Rope(newCapstan.line);
        }

        public void settings_setup()
        {
            // part of preparing Dock object to be saved in settings by resetting certain information
            line.settings_setup();
            force = new myVector(0, 0);
            speed = 0;
            set_speed = 0;
            T_hold = 0;
            n_turns = 0;
            brake = false;
            slip = false;
        }

        public void update_pos(double _x, double _y)
        {
            // set coordinates of capstan
            pos.equals(_x, _y);
        }

        public void update_attach_point(double _x, double _y)
        {
            // update where capstan attach point is in space
            line.attach_point.equals(_x, _y);
        }

        public void update_rope_direction(myVector ship_pos, double ship_angle)
        {
            // update where line is pulling the ship from
            line.update_direction(pos, ship_pos, ship_angle);
        }

        public void update_T_hold(double T)
        {
            // change how much force a dock worker puts on its rope
            T_hold = T;
        }

        public void update_set_speed(double s)
        {
            // update speed capstan will try to maintain
            set_speed = s;
            speed = set_speed;
        }

        public void update_n_turns(double _n_turns)
        {
            // change the number of turns of rope around the capstan
            n_turns = _n_turns;
        }

        public void increase_n_turns()
        {
            n_turns++;
        }

        public void decrease_n_turns()
        {
            n_turns--;
            if (n_turns < 0) { n_turns = 0; }

        }

        public void set_light_hold()
        {
            T_hold = light_hold;
        }
        public void set_strong_hold()
        {
            T_hold = strong_hold;
        }

        private void update_slack(myVector ship_velocity, double ship_angular_velocity, double dt)
        {
            // slack change is the difference between ship speed relative to capstan and current capstan speed
            ship_velocity.y += line.attach_point.x * ship_angular_velocity;
            ship_velocity.x -= line.attach_point.y * ship_angular_velocity;
            line.slack += (line.direction.project(ship_velocity) - speed) * dt;
        }

        private void VFD_control()
        {
            // depending on force seen by capstan, modify capstan behavior
            if (force.total() > F_stall)
            {
                speed = 0;
                brake = true;
            }
            else
            {
                speed = set_speed;
            }
        }

        public void update_F_MAX()
        {
            // update the force to: max static friction or kinetic friction
            if (slip)
            {
                F_MAX = (Pow(E, coeff_friction_k * n_turns * 2 * PI) - 1) * T_hold;
            }
            else
            {
                F_MAX = (Pow(E, coeff_friction_s * n_turns * 2 * PI) - 1) * T_hold;
            }
        }

        public double moment()
        {
            // definition of moment
            return line.attach_point.cross(force); //could replace this with force from capstan
        }

        public void update(myVector ship_velocity, myVector ship_pos, double ship_angular_velocity, double ship_angle, double dt)
        {
            // logic to determine force exerted by the capstan on the ship
            line.update_direction(pos, ship_pos, ship_angle);
            update_slack(ship_velocity, ship_angular_velocity, dt);
            update_F_MAX();

            if (line.slack > 0 && line.stretch <= 0)
            {
                line.slack += -line.stretch;
                line.stretch = 0;
                force.equals(0, 0);
            }
            else if (T_hold <= 0) // rope is taut
            {
                force.equals(0, 0);
                line.update_direction(pos, ship_pos, ship_angle);
                line.slack = 0;
                slip = true;
            }
            else // rope is taut and a force is applied by dock worker
            {
                line.stretch += (-1 * line.slack);
                line.slack = 0;
                line.update_tension();

                if (line.tension > F_MAX && slip == false)
                {
                    // line slips off of capstan and isn't already slipping
                    slip = true;
                    update_F_MAX(); // new force from kinetic friction
                    line.update_stretch(F_MAX); //line instantaneously shrinks
                    force = line.direction.multiply(line.tension);
                }
                else
                {
                    // check if capstan stops slipping

                    force = line.direction.multiply(line.tension);
                    VFD_control();
                }
            }
        }

    }

    [Serializable]
    public class Ship
    {
        public double angle = 0;
        public double omega = 0;
        public double m = 1000000;
        public double I = 1000;
        public myVector pos = new myVector(0, 0);
        public myVector velocity = new myVector(0, 0);
        public myVector dimensions = new myVector(50, 340);

        public Ship(double xpos, double ypos, double mass)
        {
            m = mass;
            pos.equals(xpos, ypos);
            I = (1 / 12) * mass * (Pow(dimensions.x, 2) + Pow(dimensions.y, 2));
        }

        public Ship(Ship newShip)
        {
            angle = newShip.angle;
            if (newShip.m == 0)
            {
                m = 100000;
            }
            else
            {
                m = newShip.m;
            }
            if (newShip.I == 0)
            {
                I = 100000;
            }
            else
            {
                I = newShip.I;
            }
            

            pos = new myVector(newShip.pos);
            velocity = new myVector(newShip.velocity);
            dimensions = newShip.dimensions;
        }

        public void settings_setup()
        {
            // prepare Ship object in Dock for settings export to JSON
        }

        public void update_pos(double xpos, double ypos)
        {
            pos.equals(xpos, ypos);
        }

        public void apply_force(myVector f, double dt)
        {
            double a_x = f.x / m;
            double a_y = f.y / m;
            double dx = velocity.x * dt + 0.5 * a_x * Pow(dt, 2);
            double dy = velocity.y * dt + 0.5 * a_y * Pow(dt, 2);
            double x = pos.x + dx;
            double y = pos.y + dy;
            update_pos(x, y);

            x = velocity.x + a_x * dt;
            y = velocity.y + a_y * dt;

            velocity.equals(x, y);
            velocity.update_theta();
        }

        public void apply_moment(double M, double dt)
        {
            double alpha = (M / I);
            angle += omega * dt + 0.5 * alpha * Pow(dt, 2);
            omega += alpha * dt;
        }

    }

    [Serializable]
    public class KeelBlock
    {
        public myVector global_position;
        public myVector local_position;
        private myVector point_on_ship;
        public myVector diff;

        public double width;
        public double x_tolerance = 0.001;
        public double y_tolerance = 0.1;

        public KeelBlock()
        {
            global_position = new myVector(0, 0);
            local_position = new myVector(0, 0);
            point_on_ship = local_position;
        }

        public KeelBlock(double x_pos, double y_pos, myVector drydock_dimensions)
        {
            local_position = new myVector(x_pos, y_pos);
            point_on_ship = local_position;
            point_on_ship.update_theta();
            global_position = new myVector(drydock_dimensions.x / 2 + x_pos, drydock_dimensions.y / 2 + y_pos);
            Debug.Log("global position x: " + global_position.x + " global position y: " + global_position.y);
        }

        public KeelBlock(KeelBlock newKeelBlock)
        {
            local_position = newKeelBlock.local_position;
            global_position = newKeelBlock.global_position;
            point_on_ship = local_position;
        }

        public bool IsCentered(myVector ship_pos, double ship_angle)
        {
            point_on_ship.rotate(ship_angle);

            diff = global_position - (point_on_ship + ship_pos);

            //Debug.Log("x difference: " + diff.x + " y difference: " +  diff.y);

            if (Abs(diff.x) <= x_tolerance && Abs(diff.y) <= y_tolerance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    [Serializable]
    public class Wind
    {
        public double height = 76.2;
        public double density = 1.225;
        public double coeff = 1;
        public double speed = 1;
        private double current_speed = 0;
        public myVector direction = new myVector(0, 0);
        public myVector force = new myVector(0, 0);

        private double k;
        private double wind_accel = 1;

        public Wind(double r, double theta)
        {
            double x = r * Cos(theta);
            double y = r * Sin(theta);

            double hypotenuse = Sqrt(Pow(x, 2) + Pow(y, 2));

            direction.equals(x / hypotenuse, y / hypotenuse);
            direction.r = 1;

            double angle = theta + PI / 2;

            double length = 2 * Sqrt(700 / (Pow(Cos(angle), 2)) + (1 / 64) * Pow(Sin(angle), 2));
            k = 0.5 * coeff * density * height * length;
        }

        public Wind(Wind newWind)
        {
            height = newWind.height;
            density = newWind.density;
            coeff = newWind.coeff;
            speed = newWind.speed;
        }

        private void VarySpeed()
        {
            int max = 1000;
            int min = -1000;
            System.Random rnd = new System.Random();
            current_speed += rnd.Next(min, max) * wind_accel;
        }

        public myVector update()
        {
            force = direction.multiply(k * Pow(current_speed, 2));
            VarySpeed();
            return force;
        }

    }

    [Serializable]
    public class Water
    {
        public double coeff = 1;
        public double density = 1000;
        public double water_depth = 12.2;
        public double moment = 0;
        public myVector force = new myVector(0, 0);

        public Water()
        {

        }
        public Water(Water newWater)
        {
            coeff = newWater.coeff;
            density = newWater.density;
            water_depth = newWater.water_depth;
        }
        
        public double update_moment(double ship_angular_velocity, double ship_length)
        {
            moment = -Sign(ship_angular_velocity) *  (1 / 8) * coeff * density * water_depth * Pow(ship_angular_velocity, 2) * Pow(ship_length, 4);
            return moment;
        }
        public myVector update_force(myVector ship_velocity)
        {
            double angle = ship_velocity.theta + PI / 2;
            double length;
            length = 2 * Sqrt(700 / (Pow(Cos(angle), 2)) + (1 / 64) * Pow(Sin(angle), 2));

            force.r = -0.5 * coeff * density * water_depth * length * Pow(ship_velocity.r, 2);
            force.x = force.r * Cos(ship_velocity.theta);
            force.y = force.r * Sin(ship_velocity.theta);
            return force;
        }
    }

    [Serializable]
    public class Dock
    {
        int n = 0;
        public myVector dimensions = new myVector(0, 0);
        public Capstan[] capstans;
        public KeelBlock[] keel_blocks;
        public Ship ship;
        public Wind gust;
        public Water drag;

        public Dock(int n, double x_dim, double y_dim)
        {
            capstans = new Capstan[n];
            for (int i = 0; i < capstans.Length; i++)
            {
                capstans[i] = new Capstan(i + 1);
            }
            keel_blocks = new KeelBlock[3];
            dimensions.equals(x_dim, y_dim);
            ship = new Ship(0, 0, 0);
            gust = new Wind(0, 0);
            drag = new Water();
        }

        public Dock(int n, double x_dim, double y_dim, Ship _ship)
        {
            capstans = new Capstan[n];
            dimensions.equals(x_dim, y_dim);
            ship = _ship;
            gust = new Wind(0, 0);
            drag = new Water();
        }

        public Dock(Ship _ship, Capstan[] _capstans)
        {
            capstans = _capstans;
            ship = _ship;
            gust = new Wind(0, 0);
            drag = new Water();
            AutoDimension();
        }

        public Dock(Dock newDock)
        {
            drag = new Water(newDock.drag);
            gust = new Wind(newDock.gust);
            ship = new Ship(newDock.ship);
            keel_blocks = new KeelBlock[newDock.keel_blocks.Length];
            for (int i = 0; i < newDock.keel_blocks.Length; i++)
            {
                keel_blocks[i] = new KeelBlock(newDock.keel_blocks[i]);
            }
            dimensions = newDock.dimensions;
            n = newDock.n;
            capstans = new Capstan[newDock.capstans.Length];
            for (int i = 0; i < newDock.capstans.Length; i++)
            {
                capstans[i] = new Capstan(newDock.capstans[i]);
                capstans[i].update_rope_direction(ship.pos, ship.angle);
            }
            AutoDimension();
        }

        private void AutoDimension()
        {
            dimensions.x = Abs(capstans[0].pos.x - capstans[capstans.Length - 1].pos.x);
            if (dimensions.x == 0)
            {
                dimensions.x = Abs(capstans[0].pos.x - capstans[(int)(capstans.Length / 2) + 1].pos.x);
            }
            dimensions.y = Abs(capstans[0].pos.y - capstans[capstans.Length - 1].pos.y);
            if (dimensions.y == 0)
            {
                dimensions.y = Abs(capstans[0].pos.y - capstans[(int)(capstans.Length / 2) + 1].pos.y);
            }
        }

        public void update(double dt)
        {
            myVector net_force = new myVector(0, 0);

            foreach (Capstan c in capstans)
            {
                c.update(ship.velocity, ship.pos, ship.omega, ship.angle, dt);
                net_force.add(c.force);
            }
            net_force.add(gust.update());
            //net_force.add(drag.update_force(ship.velocity));
            ship.apply_force(net_force, dt);

            double net_moment = 0;
            foreach (Capstan c in capstans)
            {
                net_moment += c.moment();
            }
            //net_moment += drag.update_moment(ship.omega, 2 * ship.dimensions.y);
            ship.apply_moment(net_moment, dt);

        }
    }
}