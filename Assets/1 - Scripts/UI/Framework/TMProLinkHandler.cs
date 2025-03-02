using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace Edgeflow.UI
{
    public class TMProLinkHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public System.Action<string> HoverStart;
        public System.Action<string> HoverEnd;
        public System.Action<string> Click;

        private bool _isHoveringObject;

        private TextMeshProUGUI m_TextMeshPro;
        private RectTransform m_TextPopup_RectTransform;
        private Canvas m_Canvas;
        private Camera m_Camera;
        private int m_selectedLink = -1;

        // Start is called before the first frame update
        void Awake()
        {
        }

        private void OnDisable()
        {
            //if( m_TextMeshPro )
            //    m_TextPopup_RectTransform.gameObject.SetActive( false );
        }

        private void Start()
        {
            m_Canvas = gameObject.GetComponentInParent<Canvas>();

            // Get a reference to the camera if Canvas Render Mode is not ScreenSpace Overlay.
            if( m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay )
                m_Camera = null;
            else
                m_Camera = m_Canvas.worldCamera;

            m_TextMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if( _isHoveringObject )
            {
                // Check if mouse intersects with any links.
                int linkIndex = TMP_TextUtilities.FindIntersectingLink( m_TextMeshPro, Input.mousePosition, m_Camera );

                //Done 
                // Clear previous link selection if one existed.
                if( ( linkIndex == -1 && m_selectedLink != -1 ) || linkIndex != m_selectedLink )
                {
                    if( m_selectedLink != -1 )
                    {
                        TMP_LinkInfo linkInfo = m_TextMeshPro.textInfo.linkInfo[m_selectedLink];
                        //m_TextPopup_RectTransform.gameObject.SetActive( false );
                        HoverEnd?.Invoke( linkInfo.GetLinkID() );
                    }
                    m_selectedLink = -1;
                }

                // Handle new Link selection.
                if( linkIndex != -1 && linkIndex != m_selectedLink )
                {
                    m_selectedLink = linkIndex;

                    TMP_LinkInfo linkInfo = m_TextMeshPro.textInfo.linkInfo[linkIndex];

                    // Debug.Log("Link ID: \"" + linkInfo.GetLinkID() + "\"   Link Text: \"" + linkInfo.GetLinkText() + "\""); // Example of how to retrieve the Link ID and Link Text.

                    Vector3 top = m_TextMeshPro.transform.TransformPoint( new Vector3( 0f, m_TextMeshPro.rectTransform.rect.max.y, 0f ) );
                    top = RectTransformUtility.WorldToScreenPoint( m_Camera, top );
                    Vector3 mousePos = Input.mousePosition;
                    mousePos.y = top.y;

                    Vector3 worldPointInRectangle;
                    RectTransformUtility.ScreenPointToWorldPointInRectangle( m_TextMeshPro.rectTransform, mousePos, m_Camera, out worldPointInRectangle );

                    string linkID = linkInfo.GetLinkID();

                    HoverStart?.Invoke( linkInfo.GetLinkID() );

                    //m_TextPopup_RectTransform.position = worldPointInRectangle;
                }

                if( m_selectedLink != -1 )
                {
                    if( Input.GetMouseButtonDown( 0 ) )
                    {
                        TMP_LinkInfo linkInfo = m_TextMeshPro.textInfo.linkInfo[linkIndex];
                        Click?.Invoke( linkInfo.GetLinkID() );
                    }
                }
            }
        }

        void OnDestroy()
        {
        }

        public void OnPointerEnter( PointerEventData eventData )
        {
            _isHoveringObject = true;
        }


        public void OnPointerExit( PointerEventData eventData )
        {
            _isHoveringObject = false;

            //m_TextPopup_RectTransform.gameObject.SetActive( false );
            m_selectedLink = -1;
        }



    }
}