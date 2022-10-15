using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game3Manager : MonoBehaviour
{
    public List<GameObject> ghostPrefabs;
    public TMPro.TextMeshProUGUI GuideText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Touch processing
        if (Input.touchCount > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //GuideText.text = $"슬라이더를 움직여 유령의 모양을 바꿔서 원래 소리대로 맞춰보세요.\n";
                //if (currentGhost == null)
                //{
                //    currentGhost = hit.transform.gameObject;
                //    var fmodInstance = FMODUnity.RuntimeManager.CreateInstance($"event:/3D/Ghost{currentIndex + 1}");
                //    ghost = currentGhost.GetComponent<Ghost>();
                //    ghost.Init(fmodInstance);
                //}

                //if (!ghost.isPlaying)
                //{
                //    ghost.play();
                //}
                //// selection 표시
                //currentGhost.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }
}
