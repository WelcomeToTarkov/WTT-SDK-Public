using UnityEngine;

namespace EFT.Visual
{
    public class HelmetDress : MonoBehaviour
    {

        public Quaternion OffRotation;

        public Quaternion OnRotation;

        public Transform HingeTransform;

        public HysteresisFilter Filter;

    }
}