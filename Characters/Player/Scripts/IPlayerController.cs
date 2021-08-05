using UnityEngine;

interface IPlayerController {
    void Follow(Transform target);
    bool isFollowing { get; }
}