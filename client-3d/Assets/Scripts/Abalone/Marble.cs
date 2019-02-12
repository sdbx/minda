using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abalone
{
    public class Marble : MonoBehaviour
    {
        public GameContext context;

        public AxialCoord arrayPosition { get; private set; }
        public CubeCoord visiblePosition => arrayPosition + boardSettings.placementOffset;

        public AnimationCurve yCurve;

        public int playerIndex { get; private set; }
        public bool fallen;
        private float t = 0.0f;
        private new Renderer renderer;
        private Material defaultMaterial;
        private Material skinMaterial;
        private BoardSettings boardSettings;

        [SerializeField] private Color overColor;
        [SerializeField] private Color selectColor;
        [SerializeField] private Material[] materials;
        private Color originalColor;

        private void Awake()
        {
            renderer = GetComponent<Renderer>();
        }

        public IEnumerator HelloWorld()
        {
            renderer.material.color = Color.black;
            yield return new WaitForSeconds(0.05f);
            renderer.material.color = originalColor;
            yield return new WaitForSeconds(0.05f);
            renderer.material.color = Color.black;
            yield return new WaitForSeconds(0.05f);
            renderer.material.color = originalColor;
            yield return new WaitForSeconds(0.05f);
        }

        private void OnMouseOver()
        {
            if (context.CanPaintOver && context.currentPlayerIndex == playerIndex && !fallen)
            {
                //renderer.material = defaultMaterial;
                renderer.material.color = overColor;
            }
        }

        private void OnMouseExit()
        {
            if (context.CanPaintOver && context.currentPlayerIndex == playerIndex && !fallen)
            {
                //renderer.material = skinMaterial;
                renderer.material.color = originalColor;
            }
        }

        public void Init(BoardSettings boardSettings, Color color, Material material, AxialCoord arrayPosition, int playerIndex, GameContext gameContext)
        {
            this.boardSettings = boardSettings;
            this.playerIndex = playerIndex;
            context = gameContext;
            defaultMaterial = renderer.material;
            skinMaterial = material;
            renderer.material = skinMaterial;
            //SetColor(color);
            SetColor(Color.white);
            SetPosition(arrayPosition);
        }

        public void SetColor(Color color)
        {
            originalColor = color;
            renderer.material.color = color;
        }

        public void SetPosition(AxialCoord arrayPosition)
        {
            this.arrayPosition = arrayPosition;
        }

        public void SetArrayPosition(AxialCoord axialCoord)
        {
            arrayPosition = axialCoord;
        }

        public void PaintOrigin(bool over)
        {
            context.CanPaintOver = over;
            //renderer.material = skinMaterial;
            renderer.material.color = originalColor;
        }

        public void PaintSelectColor()
        {
            context.CanPaintOver = false;
            //renderer.material = defaultMaterial;
            renderer.material.color = selectColor;
        }

        public void FallAnimation(Vector3 fallDirection)
        {
            GetComponent<Rigidbody>().isKinematic = false;
            fallen = true;
            context.fallenMarbles[playerIndex - 1]++;
            StartCoroutine(FallCoroutine(fallDirection));
        }

        public Vector3 DragLimit(Vector3 marble, Vector3 mouse)
        {
            //-8.5 ~ -5.5 -3.3~3.3 5.5 ~ 8.5
            GetComponent<Rigidbody>().isKinematic = true;

            if (marble.z >= -8.5f && marble.z <= -5.3f)
            {
                if(marble.x > 0)
                    return new Vector3(Mathf.Min(6.8f, Mathf.Max(1.5f, mouse.x)), marble.y, (Mathf.Min(6.8f, Mathf.Max(1.5f, mouse.x)) - 1.5f) / Mathf.Sqrt(3) - 8.5f);
                if (marble.x < 0)
                    return new Vector3(Mathf.Min(-1.5f, Mathf.Max(-6.8f, mouse.x)), marble.y, (-Mathf.Min(-1.5f, Mathf.Max(-6.8f, mouse.x)) - 1.5f) / Mathf.Sqrt(3) - 8.5f);
            }
            if (marble.z >= -3.3f && marble.z <= 3.3f)
                return new Vector3(marble.x, marble.y, Mathf.Min(3.4f, Mathf.Max(-3.4f, mouse.z)));

            if (marble.z >= 5.3f && marble.z <= 8.5f)
            {
                if (marble.x > 0)
                    return new Vector3(Mathf.Min(6.8f, Mathf.Max(1.5f, mouse.x)), marble.y, (-Mathf.Min(6.8f, Mathf.Max(1.5f, mouse.x)) + 1.5f) / Mathf.Sqrt(3) + 8.5f);
                if (marble.x < 0)
                    return new Vector3(Mathf.Min(-1.5f, Mathf.Max(-6.8f, mouse.x)), marble.y, (Mathf.Min(-1.5f, Mathf.Max(-6.8f, mouse.x)) + 1.5f) / Mathf.Sqrt(3) + 8.5f);
            }

            return marble;
        }

        private IEnumerator FallCoroutine(Vector3 fallDirection)
        {
            const float deltaT = 0.007f;
            while (true)
            {
                t += deltaT;
                transform.localPosition += t * fallDirection;
                if (t > 0.08)
                    yield break;

                yield return null;
            }
        }
    }
}
