using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public InputAction CardSelectAction;
    public float DragSpeed = 10f;

    Card _selectedCard;
    WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate(); 

    void OnEnable()
    {
        CardSelectAction.Enable();
        CardSelectAction.performed += OnCardSelected;
    }

    void OnDisable()
    {
        CardSelectAction.performed -= OnCardSelected;
        CardSelectAction.Disable();
    }
    
    void OnCardSelected(InputAction.CallbackContext ctx)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            if(hit.collider != null)
            {
                hit.collider.TryGetComponent<Rigidbody>(out Rigidbody rb);
                hit.collider.TryGetComponent<Card>(out _selectedCard);
                if(!_selectedCard.owner.isActive) return;
                if(rb != null) StartCoroutine(DragUpdate(rb));
                hit.collider.TryGetComponent<Card>(out _selectedCard);
                if(_selectedCard != null) _selectedCard.state = Card.State.Selected;
            }
        }
    }

    IEnumerator DragUpdate(Rigidbody obj)
    {
        float initalDistance = Vector3.Distance(obj.transform.position, Camera.main.gameObject.transform.position);
        while(CardSelectAction.ReadValue<float>() != 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Vector3 direction = ray.GetPoint(initalDistance) - obj.transform.position;
            obj.velocity = direction * DragSpeed;
            yield return waitForFixedUpdate;
        }
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
