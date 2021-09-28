using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static bool MeshSweepTest(List<Triangle> triangles, Vector3 point, Vector3 direction)
    {
        int sum = 0;
        foreach (var triangle in triangles)
            sum += triangle.SweepTest(point, direction);
        return sum == 0;
    }

    public static Vector3 RandomVector(System.Random random)
    {
        return new Vector3(((float)random.NextDouble()), ((float)random.NextDouble()), ((float)random.NextDouble()));
    }

    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2, float epsilon = 0.0001f)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parrallel
        if (Mathf.Abs(planarFactor) < epsilon && crossVec1and2.sqrMagnitude > epsilon)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }
    public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
    {

        //get vector from point on line to point in space
        Vector3 linePointToPoint = point - linePoint;

        float t = Vector3.Dot(linePointToPoint, lineVec);

        return linePoint + lineVec * t;
    }
    public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point){
 
		Vector3 lineVec = linePoint2 - linePoint1;
		Vector3 pointVec = point - linePoint1;
 
		float dot = Vector3.Dot(pointVec, lineVec);
 
		//point is on side of linePoint2, compared to linePoint1
		if(dot > 0){
 
			//point is on the line segment
			if(pointVec.magnitude <= lineVec.magnitude){
 
				return 0;
			}
 
			//point is not on the line segment and it is on the side of linePoint2
			else{
 
				return 2;
			}
		}
 
		//Point is not on side of linePoint2, compared to linePoint1.
		//Point is not on the line segment and it is on the side of linePoint1.
		else{
 
			return 1;
		}
	}
    public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
    {

        Vector3 vector = linePoint2 - linePoint1;

        Vector3 projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);

        int side = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint);

        //The projected point is on the line segment
        if (side == 0)
        {

            return projectedPoint;
        }

        if (side == 1)
        {

            return linePoint1;
        }

        if (side == 2)
        {

            return linePoint2;
        }

        //output is invalid
        return Vector3.zero;
    }
    public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;

        float a = Vector3.Dot(lineVec1, lineVec1);
        float b = Vector3.Dot(lineVec1, lineVec2);
        float e = Vector3.Dot(lineVec2, lineVec2);

        float d = a * e - b * b;

        //lines are not parallel
        if (d != 0.0f)
        {

            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }

        else
        {
            return false;
        }
    }

    public static float MouseDistanceToLine(Vector3 linePoint1, Vector3 linePoint2)
    {
        Camera currentCamera;
        Vector3 mousePosition;

#if UNITY_EDITOR
        if (Camera.current != null)
        {

            currentCamera = Camera.current;
        }

        else
        {

            currentCamera = Camera.main;
        }

        //convert format because y is flipped
        mousePosition = new Vector3(Event.current.mousePosition.x, currentCamera.pixelHeight - Event.current.mousePosition.y, 0f);

#else
		currentCamera = Camera.main;
		mousePosition = Input.mousePosition;
#endif

        Vector3 screenPos1 = currentCamera.WorldToScreenPoint(linePoint1);
        Vector3 screenPos2 = currentCamera.WorldToScreenPoint(linePoint2);
        Vector3 projectedPoint = ProjectPointOnLineSegment(screenPos1, screenPos2, mousePosition);

        //set z to zero
        projectedPoint = new Vector3(projectedPoint.x, projectedPoint.y, 0f);

        Vector3 vector = projectedPoint - mousePosition;
        return vector.magnitude;
    }
}
