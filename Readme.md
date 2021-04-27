
プロジェクト作成
２Dのテンプレートを利用して作成する

キャラクターのセットアップから行います
指定のキャラテクスチャをインポート

SpriteMode
 Multipleに変更
 FullRectに変更
 Apply後にSpriteEditor

SpriteEditorではスライス設定を行う
Grid by Cell Size
200x200
2x2
0x2
Pivot Custorm 0.5x0.2

スプライトchara00101_mini_54をシーン内にドロップ
キャラクターのベースとして利用

AddComponent
 +RigidBody2D
 +CapsuleCollider2D

RigidBody2D
GravityScale 0
Constrains rotate freeze

CapsuleCollider2D
offset(0 , 0.6)
size(1 , 1.8)

入力システムを作る
Window/PackageManager
All PackagesにしてInputSystemを探す
Install
なんか警告がでてもyes

Unit操作用のスクリプトを作成
UnitController.cs

using UnityEngine.InputSystem;

public class UnitController : MonoBehaviour
{
	private Rigidbody2D m_rbBody;
	private Transform m_tfAvator;

	[SerializeField]
	InputAction m_WASD;
	private Vector2 m_movementInput;
	[SerializeField]
	private float m_fMovementSpeed;

	private void OnEnable()
	{
		m_WASD.Enable();
	}
	private void OnDisable()
	{
		m_WASD.Disable();
	}

	private void Start()
	{
		m_rbBody = GetComponent<Rigidbody2D>();
		m_tfAvator = transform.GetChild(0);
	}

	private void Update()
	{
		m_movementInput = m_WASD.ReadValue<Vector2>();
	}
	private void FixedUpdate()
	{
		m_rbBody.velocity = m_fMovementSpeed * m_movementInput;
	}
}

作れたらUnitのインスペクターにスクリプトを貼り付ける









