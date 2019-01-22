using UnityEngine;

namespace SceneAssets.Common.TeleOperation
{
	public class FrankaTeleOpReceiver : MonoBehaviour {
		
		[SerializeField] Transform target_pose;
		[SerializeField] Transform default_pose;

		public void SetRelativeTargetEulerPose(Vector3 relativePosition, Vector3 relativeForward){
			target_pose.position = default_pose.TransformPoint(relativePosition);
			target_pose.up = default_pose.TransformDirection(relativeForward);
		}

		public void SetRelativeTargetQuaternionPose(Vector3 relativePosition, Quaternion relativeForward){
			target_pose.position = default_pose.TransformPoint(relativePosition);
			target_pose.rotation = default_pose.rotation * relativeForward;
		}
	}
}
