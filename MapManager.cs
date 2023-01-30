using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/*----------------------------------------------*/
// 製作者 : 金盛 翔
// 概要   : ゲームそのものを管理するスクリプト
// 進捗   : 完成
/*----------------------------------------------*/
public class MapManager : MonoBehaviour
{
    // タイルの種類
    private enum TileType
    {
        // 何も無い
        NONE, 
        // 地面
        GROUND, 
        // 目的地
        TARGET, 
        // プレイヤー
        PLAYER, 
        // ブロック
        BLOCK, 
        // プレイヤー（目的地の上）
        PLAYER_ON_TARGET, 
        // ブロック（目的地の上）
        BLOCK_ON_TARGET, 
    }

    [SerializeField, Header("ステージ構造が記述されたテキストファイル")]
    private TextAsset _stageFile;

    // 行数
    private int _stageRows;

    // 列数
    private int _stageColumns; 

    [SerializeField, Header("タイル情報を管理する二次元配列")]
    private TileType[,] _tiles;

    [SerializeField, Header("タイルのサイズ")]
    private float _tileSize;

    [SerializeField, Header("地面のスプライト")]
    private Sprite _groundSprite;

    [SerializeField, Header("目的地のスプライト")]
    private Sprite _targetSprite;

    [SerializeField, Header("プレイヤーのスプライト")]
    private Sprite _playerSprite;

    [SerializeField, Header("ブロックのスプライト")]
    private Sprite _blockSprite;

    [SerializeField, Header("プレイヤーオブジェクト")] 
    private GameObject _playerObj;

    // タイルの中心座標
    private Vector2 _tileCenterPosition;

    // ブロックの数
    private int _blockCount; 

    //クリア判定
    private bool _isClear;

    [SerializeField, Header("ゲームクリアのUI")]
    private GameObject _clearUI = default;

    // 各位置に存在するゲームオブジェクトを管理する連想配列
    private Dictionary<GameObject, Vector2Int> _gameObjectPosTable = new Dictionary<GameObject, Vector2Int>();


    private void Awake()
    {
        // タイルの情報を読み込む
        LoadTileData(); 
        // ステージを作成
        CreateStage(); 
    }

    /// <summary>
    /// <para>LoadTileData</para>
    /// <para>タイルの情報を読み込む</para>
    /// </summary>
    private void LoadTileData()
    {
        // タイルの情報を一行ごとに分割
        string[] lines = SplitMapData();

        // タイルの列数を計算
        string[] rows = lines[0].Split(new[] { ',' });

        // タイルの列数と行数を保持
        // 行数
        _stageRows = lines.Length; 
        // 列数
        _stageColumns = rows.Length; 

        // タイル情報を int 型の２次元配列で保持
        _tiles = new tileType[_stageColumns, _stageRows];
        for (int y = 0; y < _stageRows; y++)
        {
            // 一文字ずつ取得
            string st = lines[y];
            rows = st.Split(new[] { ',' });
            for (int x = 0; x < _stageColumns; x++)
            {
                // 読み込んだ文字を数値に変換して保持
                _tiles[x, y] = (tileType)int.Parse(rows[x]);
            }
        }
    }

    /// <summary>
    /// <para>SplitMapData</para>
    /// <para>マップの生成処理</para>
    /// </summary>
    /// <returns>テキストファイルの内容を参照して配列に格納・返す</returns>
    private string[] SplitMapData()
    {
        //テキストファイルの内容を参照して配列に格納・返す
        return _stageFile.text.Split
                (
                    //配列にデータ上の座標の数値を格納
                    new[] { '\r', '\n' },
                    //データがない部分は除く
                    System.StringSplitOptions.RemoveEmptyEntries
                );
    }

    /// <summary>
    /// <para>CreateStage</para>
    /// <para>ステージを作成</para>
    /// </summary>
    private void CreateStage()
    {
        /** ステージの中心位置を計算
         * x軸の計算 : マップデータ上の列数数に合わせた位置に移動 - タイルサイズの半分のズレを補正
         * y軸の計算 : マップデータ上の行数数に合わせた位置に移動 - タイルサイズの半分のズレを補正
         **/
        _tileCenterPosition.x = _stageColumns * _tileSize * 0.5f - _tileSize * 0.5f;
        _tileCenterPosition.y = _stageRows * _tileSize * 0.5f - _tileSize * 0.5f; ;

        //地形データを格納している配列の行を参照
        for (int y = 0; y < _stageRows; y++)
        {
            //列を参照
            for (int x = 0; x < _stageColumns; x++)
            {
                //現在参照している座標に格納されている数値を格納
                tileType val = _tiles[x, y];

                // 何も無い場所は無視
                if (val == tileType.NONE) 
                { 
                    continue; 
                }

                // タイルの名前に行番号と列番号を付与
                string name = "tile" + y + "_" + x;

                // タイルのゲームオブジェクトを作成
                GameObject tile = new GameObject(name);

                // タイルにスプライトを描画する機能を追加
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();

                // タイルのスプライトを設定
                sr.sprite = _groundSprite;

                // タイルの位置を設定
                tile.transform.position = GetDisplayPosition(x, y);

                // 目的地の場合
                if (val == tileType.TARGET)
                {
                    // 目的地のゲームオブジェクトを作成
                    GameObject destination = new GameObject("destination");

                    // 目的地にスプライトを描画する機能を追加
                    sr = destination.AddComponent<SpriteRenderer>();

                    // 目的地のスプライトを設定
                    sr.sprite = _targetSprite;

                    // 目的地の描画順を手前にする
                    sr.sortingOrder = 1;

                    // 目的地の位置を設定
                    destination.transform.position = GetDisplayPosition(x, y);
                }
                // プレイヤーの場合
                else　if (val == tileType.PLAYER)
                {
                    // プレイヤーのゲームオブジェクトを作成
                    _playerObj = new GameObject("player");

                    // プレイヤーにスプライトを描画する機能を追加
                    sr = _playerObj.AddComponent<SpriteRenderer>();

                    // プレイヤーのスプライトを設定
                    sr.sprite = _playerSprite;

                    // プレイヤーの描画順を手前にする
                    sr.sortingOrder = 2;

                    // プレイヤーの位置を設定
                    _playerObj.transform.position = GetDisplayPosition(x, y);

                    // プレイヤーを連想配列に追加
                    _gameObjectPosTable.Add(_playerObj, new Vector2Int(x, y));
                }
                // ブロックの場合
                else if (val == tileType.BLOCK)
                {
                    // ブロックの数を増やす
                    _blockCount++;

                    /** ブロックのゲームオブジェクトを作成
                     * 名前の語尾には_blockCountを付けて番号分けする
                     **/
                    GameObject block = new GameObject("block" + _blockCount);

                    // ブロックにスプライトを描画する機能を追加
                    sr = block.AddComponent<SpriteRenderer>();

                    // ブロックのスプライトを設定
                    sr.sprite = _blockSprite;

                    // ブロックの描画順を手前にする
                    sr.sortingOrder = 2;

                    // ブロックの位置を設定
                    block.transform.position = GetDisplayPosition(x, y);

                    // ブロックを連想配列に追加
                    _gameObjectPosTable.Add(block, new Vector2Int(x, y));
                }
            }
        }
    }

    /// <summary>
    /// <para>GetDisplayPosition</para>
    /// <para>指定された列番号と行番号からスプライトの表示位置を計算して返す</para>
    /// </summary>
    /// <param name="x">配列上のx座標</param>
    /// <param name="y">配列上のy座標</param>
    /// <returns>画面上の(Unity上での)x,y座標を格納したVector2</returns>
    private Vector2 GetDisplayPosition(int x, int y)
    {
        return new Vector2
        (
            //CreateStageで計算したUnity上での座標に移動させる
            x * _tileSize - _tileCenterPosition.x,
            y * -_tileSize + _tileCenterPosition.y
        );
    }

    /// <summary>
    /// <para>GetGameObjectAtPosition</para>
    /// <para>指定された位置に存在するゲームオブジェクトを返す</para>
    /// </summary>
    /// <param name="pos">渡されたプレイヤーの座標</param>
    /// <returns>その位置に存在するゲームオブジェクトを、無ければnullを返す。</returns>
    private GameObject GetGameObjectAtPosition(Vector2Int pos)
    {
        //呼び出した時に入っているオブジェクトとその座標を連想配列から検索
        foreach (KeyValuePair<GameObject, Vector2Int> pair in _gameObjectPosTable)
        {
            // 指定された位置が見つかった場合
            if (pair.Value == pos)
            {
                // その位置に存在するゲームオブジェクトを返す
                return pair.Key;
            }
        }
        //無ければnullを返す
        return null;
    }

    /// <summary>
    /// <para>IsValidPosition</para>
    /// <para>指定された位置がステージ内なら true を返す</para>
    /// </summary>
    /// <param name="pos">渡されたプレイヤーの座標</param>
    /// <returns>ステージ外ではないと言う情報を、ステージ外の場合はfalseを返す。</returns>
    private bool IsValidPosition(Vector2Int pos)
    {
        //ステージの幅(二次元配列の列)と高さ(配列の行)を下回る or 上回っていないか
        if (0 <= pos.x && pos.x < _stageColumns && 0 <= pos.y && pos.y < _stageRows)
        {
            //ステージ外ではないと言う情報を返す
            return _tiles[pos.x, pos.y] != tileType.NONE;
        }
        //ステージ外の場合はfalseを返す
        return false;
    }

    /// <summary>
    /// <para>IsBlock</para>
    /// <para>指定された位置のタイルがブロックなら true を返す</para>
    /// </summary>
    /// <param name="pos">渡されたプレイヤーの座標</param>
    /// <returns>格納されたタイルのタイプを確認</returns>
    private bool IsBlock(Vector2Int pos)
    {
        //座標のタイルを格納
        tileType cell = _tiles[pos.x, pos.y];

        //格納されたタイルのタイプを確認
        return cell == tileType.BLOCK || cell == tileType.BLOCK_ON_TARGET;
    }

    private void Update()
    {
        // ゲームクリアしている場合は操作できないようにする
        if (_isClear)
        {
            if (_clearUI.activeSelf == false)
            {
                //ゲームクリアのUIを表示
                _clearUI.SetActive(true);
            }

            //何らかのキーを押すと
            if (Input.anyKeyDown)
            {
                //タイトルに戻る
                SceneManager.LoadScene("Title");
            }
        }
        //ゲームプレイ中(クリアしていない)なら操作を受け付ける
        else
        {

            // 上矢印が押された場合
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // プレイヤーが上に移動できるか検証
                TryMovePlayer(DirectionType.UP);
            }
            // 右矢印が押された場合
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                // プレイヤーが右に移動できるか検証
                TryMovePlayer(DirectionType.RIGHT);
            }
            // 下矢印が押された場合
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // プレイヤーが下に移動できるか検証
                TryMovePlayer(DirectionType.DOWN);
            }
            // 左矢印が押された場合
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // プレイヤーが左に移動できるか検証
                TryMovePlayer(DirectionType.LEFT);
            }
            //Zキーが押されたら
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                //シーンのリロード
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    /// <summary>
    /// <para>TryMovePlayer</para>
    /// <para>指定された方向にプレイヤーが移動できるか検証する。移動できる場合は移動する</para>
    /// </summary>
    /// <param name="direction">上下左右の入力</param>
    private void TryMovePlayer(DirectionType direction)
    {
        // プレイヤーの現在地を取得
        Vector2Int currentPlayerPos = _gameObjectPosTable[_playerObj];

        // プレイヤーの移動先の位置を計算
        Vector2Int nextPlayerPos = GetNextPositionAlong(currentPlayerPos, direction);

        // プレイヤーの移動先がステージ内ではない場合は無視
        if (!IsValidPosition(nextPlayerPos))
        {
            return;
        }

        // プレイヤーの移動先にブロックが存在する場合
        if (IsBlock(nextPlayerPos))
        {
            // ブロックの移動先の位置を計算
            Vector2Int nextBlockPos = GetNextPositionAlong(nextPlayerPos, direction);

            // ブロックの移動先がステージ内の場合かつブロックの移動先にブロックが存在しない場合
            if (IsValidPosition(nextBlockPos) && !IsBlock(nextBlockPos))
            {
                // 移動するブロックを取得
                GameObject block = GetGameObjectAtPosition(nextPlayerPos);

                // プレイヤーの移動先のタイルの情報を更新
                UpdateGameObjectPosition(nextPlayerPos);

                // ブロックを移動
                block.transform.position = GetDisplayPosition(nextBlockPos.x, nextBlockPos.y);

                // ブロックの位置を更新
                _gameObjectPosTable[block] = nextBlockPos;

                // ブロックの移動先の番号を更新
                if (_tiles[nextBlockPos.x, nextBlockPos.y] == tileType.GROUND)
                {
                    // 移動先が地面ならブロックの番号に更新
                    _tiles[nextBlockPos.x, nextBlockPos.y] = tileType.BLOCK;
                }
                // ブロックの移動先がターゲットなら
                else if (_tiles[nextBlockPos.x, nextBlockPos.y] == tileType.TARGET)
                {
                    // 移動先が目的地ならブロック（目的地の上）の番号に更新
                    _tiles[nextBlockPos.x, nextBlockPos.y] = tileType.BLOCK_ON_TARGET;
                }

                // プレイヤーの現在地のタイルの情報を更新
                UpdateGameObjectPosition(currentPlayerPos);

                // プレイヤーを移動
                _playerObj.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

                // プレイヤーの位置を更新
                _gameObjectPosTable[_playerObj] = nextPlayerPos;

                // プレイヤーの移動先の番号を更新
                if (_tiles[nextPlayerPos.x, nextPlayerPos.y] == tileType.GROUND)
                {
                    // 移動先が地面ならプレイヤーの番号に更新
                    _tiles[nextPlayerPos.x, nextPlayerPos.y] = tileType.PLAYER;
                }
                else if (_tiles[nextPlayerPos.x, nextPlayerPos.y] == tileType.TARGET)
                {
                    // 移動先が目的地ならプレイヤー（目的地の上）の番号に更新
                    _tiles[nextPlayerPos.x, nextPlayerPos.y] = tileType.PLAYER_ON_TARGET;
                }
            }
        }
        // 或いはプレイヤーの移動先にブロックが存在しない場合
        else
        {

            // プレイヤーの現在地のタイルの情報を更新
            UpdateGameObjectPosition(currentPlayerPos);

            // プレイヤーを移動
            _playerObj.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

            // プレイヤーの位置を更新
            _gameObjectPosTable[_playerObj] = nextPlayerPos;

            // プレイヤーの移動先の番号を更新
            if (_tiles[nextPlayerPos.x, nextPlayerPos.y] == tileType.GROUND)
            {
                // 移動先が地面ならプレイヤーの番号に更新
                _tiles[nextPlayerPos.x, nextPlayerPos.y] = tileType.PLAYER;
            }
            // プレイヤーの移動先がターゲットなら
            else if (_tiles[nextPlayerPos.x, nextPlayerPos.y] == tileType.TARGET)
            {
                // 移動先が目的地ならプレイヤー（目的地の上）の番号に更新
                _tiles[nextPlayerPos.x, nextPlayerPos.y] = tileType.PLAYER_ON_TARGET;
            }
        }

        // ゲームをクリアしたかどうか確認
        CheckCompletion();
    }

    // 方向の種類
    private enum DirectionType
    {
        UP, // 上
        RIGHT, // 右
        DOWN, // 下
        LEFT, // 左
    }

    /// <summary>
    /// <para>GetNextPositionAlong</para>
    /// <para>指定された方向の位置を返す</para>
    /// </summary>
    /// <param name="pos">プレイヤーの座標</param>
    /// <param name="direction">上下左右の入力</param>
    /// <returns>移動後の座標</returns>
    private Vector2Int GetNextPositionAlong(Vector2Int pos, DirectionType direction)
    {
        switch (direction)
        {
            // 上
            case DirectionType.UP:

                //配列の参照する行を-1
                pos.y -= 1;
                //移動方向を向かせる
                _playerObj.transform.eulerAngles = new Vector3Int(0, 0, 0);
                break;

            // 右
            case DirectionType.RIGHT:

                //配列の参照する列を-1
                pos.x += 1;
                //移動方向を向かせる
                _playerObj.transform.eulerAngles = new Vector3Int(0, 0, 270);
                break;

            // 下
            case DirectionType.DOWN:
                //配列の参照する行を+1
                pos.y += 1;
                //移動方向を向かせる
                _playerObj.transform.eulerAngles = new Vector3Int(0, 0, 180);
                break;

            // 左
            case DirectionType.LEFT:
                //配列の参照する列を+1
                pos.x -= 1;
                //移動方向を向かせる
                _playerObj.transform.eulerAngles = new Vector3Int(0, 0, 90);
                break;
        }
        return pos;
    }

    /// <summary>
    /// <para>UpdateGameObjectPosition</para>
    /// <para>指定された位置のタイルを更新</para>
    /// </summary>
    /// <param name="pos">更新する座標</param>
    private void UpdateGameObjectPosition(Vector2Int pos)
    {
        // 指定された位置のタイルの番号を取得
        tileType cell = _tiles[pos.x, pos.y];

        // プレイヤーもしくはブロックの場合
        if (cell == tileType.PLAYER || cell == tileType.BLOCK)
        {
            // 地面に変更
            _tiles[pos.x, pos.y] = tileType.GROUND;
        }
        // 目的地に乗っているプレイヤーもしくはブロックの場合
        else if (cell == tileType.PLAYER_ON_TARGET || cell == tileType.BLOCK_ON_TARGET)
        {
            // 目的地に変更
            _tiles[pos.x, pos.y] = tileType.TARGET;
        }
    }

    /// <summary>
    /// <para>CheckCompletion</para>
    /// <para>ゲームをクリアしたかどうか確認</para>
    /// </summary>
    private void CheckCompletion()
    {
        // 目的地に乗っているブロックの数を計算
        int blockOnTargetCount = 0;

        //行を参照
        for (int y = 0; y < _stageRows; y++)
        {
            //列を参照
            for (int x = 0; x < _stageColumns; x++)
            {
                //その座標が「ブロックの乗ったターゲット」なら
                if (_tiles[x, y] == tileType.BLOCK_ON_TARGET)
                {
                    //「ターゲットに乗ったブロック」の数を加算
                    blockOnTargetCount++;
                }
            }
        }

        // すべてのブロックが目的地の上に乗っている場合
        if (blockOnTargetCount == _blockCount)
        {
            // ゲームクリア
            _isClear = true;
        }
    }


}

