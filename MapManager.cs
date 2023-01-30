using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/*----------------------------------------------*/
// ����� : ���� ��
// �T�v   : �Q�[�����̂��̂��Ǘ�����X�N���v�g
// �i��   : ����
/*----------------------------------------------*/
public class MapManager : MonoBehaviour
{
    // �^�C���̎��
    private enum TileType
    {
        // ��������
        NONE, 
        // �n��
        GROUND, 
        // �ړI�n
        TARGET, 
        // �v���C���[
        PLAYER, 
        // �u���b�N
        BLOCK, 
        // �v���C���[�i�ړI�n�̏�j
        PLAYER_ON_TARGET, 
        // �u���b�N�i�ړI�n�̏�j
        BLOCK_ON_TARGET, 
    }

    [SerializeField, Header("�X�e�[�W�\�����L�q���ꂽ�e�L�X�g�t�@�C��")]
    private TextAsset _stageFile;

    // �s��
    private int _stageRows;

    // ��
    private int _stageColumns; 

    [SerializeField, Header("�^�C�������Ǘ�����񎟌��z��")]
    private TileType[,] _tiles;

    [SerializeField, Header("�^�C���̃T�C�Y")]
    private float _tileSize;

    [SerializeField, Header("�n�ʂ̃X�v���C�g")]
    private Sprite _groundSprite;

    [SerializeField, Header("�ړI�n�̃X�v���C�g")]
    private Sprite _targetSprite;

    [SerializeField, Header("�v���C���[�̃X�v���C�g")]
    private Sprite _playerSprite;

    [SerializeField, Header("�u���b�N�̃X�v���C�g")]
    private Sprite _blockSprite;

    [SerializeField, Header("�v���C���[�I�u�W�F�N�g")] 
    private GameObject _playerObj;

    // �^�C���̒��S���W
    private Vector2 _tileCenterPosition;

    // �u���b�N�̐�
    private int _blockCount; 

    //�N���A����
    private bool _isClear;

    [SerializeField, Header("�Q�[���N���A��UI")]
    private GameObject _clearUI = default;

    // �e�ʒu�ɑ��݂���Q�[���I�u�W�F�N�g���Ǘ�����A�z�z��
    private Dictionary<GameObject, Vector2Int> _gameObjectPosTable = new Dictionary<GameObject, Vector2Int>();


    private void Awake()
    {
        // �^�C���̏���ǂݍ���
        LoadTileData(); 
        // �X�e�[�W���쐬
        CreateStage(); 
    }

    /// <summary>
    /// <para>LoadTileData</para>
    /// <para>�^�C���̏���ǂݍ���</para>
    /// </summary>
    private void LoadTileData()
    {
        // �^�C���̏�����s���Ƃɕ���
        string[] lines = SplitMapData();

        // �^�C���̗񐔂��v�Z
        string[] rows = lines[0].Split(new[] { ',' });

        // �^�C���̗񐔂ƍs����ێ�
        // �s��
        _stageRows = lines.Length; 
        // ��
        _stageColumns = rows.Length; 

        // �^�C������ int �^�̂Q�����z��ŕێ�
        _tiles = new tileType[_stageColumns, _stageRows];
        for (int y = 0; y < _stageRows; y++)
        {
            // �ꕶ�����擾
            string st = lines[y];
            rows = st.Split(new[] { ',' });
            for (int x = 0; x < _stageColumns; x++)
            {
                // �ǂݍ��񂾕����𐔒l�ɕϊ����ĕێ�
                _tiles[x, y] = (tileType)int.Parse(rows[x]);
            }
        }
    }

    /// <summary>
    /// <para>SplitMapData</para>
    /// <para>�}�b�v�̐�������</para>
    /// </summary>
    /// <returns>�e�L�X�g�t�@�C���̓��e���Q�Ƃ��Ĕz��Ɋi�[�E�Ԃ�</returns>
    private string[] SplitMapData()
    {
        //�e�L�X�g�t�@�C���̓��e���Q�Ƃ��Ĕz��Ɋi�[�E�Ԃ�
        return _stageFile.text.Split
                (
                    //�z��Ƀf�[�^��̍��W�̐��l���i�[
                    new[] { '\r', '\n' },
                    //�f�[�^���Ȃ������͏���
                    System.StringSplitOptions.RemoveEmptyEntries
                );
    }

    /// <summary>
    /// <para>CreateStage</para>
    /// <para>�X�e�[�W���쐬</para>
    /// </summary>
    private void CreateStage()
    {
        /** �X�e�[�W�̒��S�ʒu���v�Z
         * x���̌v�Z : �}�b�v�f�[�^��̗񐔐��ɍ��킹���ʒu�Ɉړ� - �^�C���T�C�Y�̔����̃Y����␳
         * y���̌v�Z : �}�b�v�f�[�^��̍s�����ɍ��킹���ʒu�Ɉړ� - �^�C���T�C�Y�̔����̃Y����␳
         **/
        _tileCenterPosition.x = _stageColumns * _tileSize * 0.5f - _tileSize * 0.5f;
        _tileCenterPosition.y = _stageRows * _tileSize * 0.5f - _tileSize * 0.5f; ;

        //�n�`�f�[�^���i�[���Ă���z��̍s���Q��
        for (int y = 0; y < _stageRows; y++)
        {
            //����Q��
            for (int x = 0; x < _stageColumns; x++)
            {
                //���ݎQ�Ƃ��Ă�����W�Ɋi�[����Ă��鐔�l���i�[
                tileType val = _tiles[x, y];

                // ���������ꏊ�͖���
                if (val == tileType.NONE) 
                { 
                    continue; 
                }

                // �^�C���̖��O�ɍs�ԍ��Ɨ�ԍ���t�^
                string name = "tile" + y + "_" + x;

                // �^�C���̃Q�[���I�u�W�F�N�g���쐬
                GameObject tile = new GameObject(name);

                // �^�C���ɃX�v���C�g��`�悷��@�\��ǉ�
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();

                // �^�C���̃X�v���C�g��ݒ�
                sr.sprite = _groundSprite;

                // �^�C���̈ʒu��ݒ�
                tile.transform.position = GetDisplayPosition(x, y);

                // �ړI�n�̏ꍇ
                if (val == tileType.TARGET)
                {
                    // �ړI�n�̃Q�[���I�u�W�F�N�g���쐬
                    GameObject destination = new GameObject("destination");

                    // �ړI�n�ɃX�v���C�g��`�悷��@�\��ǉ�
                    sr = destination.AddComponent<SpriteRenderer>();

                    // �ړI�n�̃X�v���C�g��ݒ�
                    sr.sprite = _targetSprite;

                    // �ړI�n�̕`�揇����O�ɂ���
                    sr.sortingOrder = 1;

                    // �ړI�n�̈ʒu��ݒ�
                    destination.transform.position = GetDisplayPosition(x, y);
                }
                // �v���C���[�̏ꍇ
                else�@if (val == tileType.PLAYER)
                {
                    // �v���C���[�̃Q�[���I�u�W�F�N�g���쐬
                    _playerObj = new GameObject("player");

                    // �v���C���[�ɃX�v���C�g��`�悷��@�\��ǉ�
                    sr = _playerObj.AddComponent<SpriteRenderer>();

                    // �v���C���[�̃X�v���C�g��ݒ�
                    sr.sprite = _playerSprite;

                    // �v���C���[�̕`�揇����O�ɂ���
                    sr.sortingOrder = 2;

                    // �v���C���[�̈ʒu��ݒ�
                    _playerObj.transform.position = GetDisplayPosition(x, y);

                    // �v���C���[��A�z�z��ɒǉ�
                    _gameObjectPosTable.Add(_playerObj, new Vector2Int(x, y));
                }
                // �u���b�N�̏ꍇ
                else if (val == tileType.BLOCK)
                {
                    // �u���b�N�̐��𑝂₷
                    _blockCount++;

                    /** �u���b�N�̃Q�[���I�u�W�F�N�g���쐬
                     * ���O�̌���ɂ�_blockCount��t���Ĕԍ���������
                     **/
                    GameObject block = new GameObject("block" + _blockCount);

                    // �u���b�N�ɃX�v���C�g��`�悷��@�\��ǉ�
                    sr = block.AddComponent<SpriteRenderer>();

                    // �u���b�N�̃X�v���C�g��ݒ�
                    sr.sprite = _blockSprite;

                    // �u���b�N�̕`�揇����O�ɂ���
                    sr.sortingOrder = 2;

                    // �u���b�N�̈ʒu��ݒ�
                    block.transform.position = GetDisplayPosition(x, y);

                    // �u���b�N��A�z�z��ɒǉ�
                    _gameObjectPosTable.Add(block, new Vector2Int(x, y));
                }
            }
        }
    }

    /// <summary>
    /// <para>GetDisplayPosition</para>
    /// <para>�w�肳�ꂽ��ԍ��ƍs�ԍ�����X�v���C�g�̕\���ʒu���v�Z���ĕԂ�</para>
    /// </summary>
    /// <param name="x">�z����x���W</param>
    /// <param name="y">�z����y���W</param>
    /// <returns>��ʏ��(Unity��ł�)x,y���W���i�[����Vector2</returns>
    private Vector2 GetDisplayPosition(int x, int y)
    {
        return new Vector2
        (
            //CreateStage�Ōv�Z����Unity��ł̍��W�Ɉړ�������
            x * _tileSize - _tileCenterPosition.x,
            y * -_tileSize + _tileCenterPosition.y
        );
    }

    /// <summary>
    /// <para>GetGameObjectAtPosition</para>
    /// <para>�w�肳�ꂽ�ʒu�ɑ��݂���Q�[���I�u�W�F�N�g��Ԃ�</para>
    /// </summary>
    /// <param name="pos">�n���ꂽ�v���C���[�̍��W</param>
    /// <returns>���̈ʒu�ɑ��݂���Q�[���I�u�W�F�N�g���A�������null��Ԃ��B</returns>
    private GameObject GetGameObjectAtPosition(Vector2Int pos)
    {
        //�Ăяo�������ɓ����Ă���I�u�W�F�N�g�Ƃ��̍��W��A�z�z�񂩂猟��
        foreach (KeyValuePair<GameObject, Vector2Int> pair in _gameObjectPosTable)
        {
            // �w�肳�ꂽ�ʒu�����������ꍇ
            if (pair.Value == pos)
            {
                // ���̈ʒu�ɑ��݂���Q�[���I�u�W�F�N�g��Ԃ�
                return pair.Key;
            }
        }
        //�������null��Ԃ�
        return null;
    }

    /// <summary>
    /// <para>IsValidPosition</para>
    /// <para>�w�肳�ꂽ�ʒu���X�e�[�W���Ȃ� true ��Ԃ�</para>
    /// </summary>
    /// <param name="pos">�n���ꂽ�v���C���[�̍��W</param>
    /// <returns>�X�e�[�W�O�ł͂Ȃ��ƌ��������A�X�e�[�W�O�̏ꍇ��false��Ԃ��B</returns>
    private bool IsValidPosition(Vector2Int pos)
    {
        //�X�e�[�W�̕�(�񎟌��z��̗�)�ƍ���(�z��̍s)������� or �����Ă��Ȃ���
        if (0 <= pos.x && pos.x < _stageColumns && 0 <= pos.y && pos.y < _stageRows)
        {
            //�X�e�[�W�O�ł͂Ȃ��ƌ�������Ԃ�
            return _tiles[pos.x, pos.y] != tileType.NONE;
        }
        //�X�e�[�W�O�̏ꍇ��false��Ԃ�
        return false;
    }

    /// <summary>
    /// <para>IsBlock</para>
    /// <para>�w�肳�ꂽ�ʒu�̃^�C�����u���b�N�Ȃ� true ��Ԃ�</para>
    /// </summary>
    /// <param name="pos">�n���ꂽ�v���C���[�̍��W</param>
    /// <returns>�i�[���ꂽ�^�C���̃^�C�v���m�F</returns>
    private bool IsBlock(Vector2Int pos)
    {
        //���W�̃^�C�����i�[
        tileType cell = _tiles[pos.x, pos.y];

        //�i�[���ꂽ�^�C���̃^�C�v���m�F
        return cell == tileType.BLOCK || cell == tileType.BLOCK_ON_TARGET;
    }

    private void Update()
    {
        // �Q�[���N���A���Ă���ꍇ�͑���ł��Ȃ��悤�ɂ���
        if (_isClear)
        {
            if (_clearUI.activeSelf == false)
            {
                //�Q�[���N���A��UI��\��
                _clearUI.SetActive(true);
            }

            //���炩�̃L�[��������
            if (Input.anyKeyDown)
            {
                //�^�C�g���ɖ߂�
                SceneManager.LoadScene("Title");
            }
        }
        //�Q�[���v���C��(�N���A���Ă��Ȃ�)�Ȃ瑀����󂯕t����
        else
        {

            // ���󂪉����ꂽ�ꍇ
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // �v���C���[����Ɉړ��ł��邩����
                TryMovePlayer(DirectionType.UP);
            }
            // �E��󂪉����ꂽ�ꍇ
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                // �v���C���[���E�Ɉړ��ł��邩����
                TryMovePlayer(DirectionType.RIGHT);
            }
            // ����󂪉����ꂽ�ꍇ
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // �v���C���[�����Ɉړ��ł��邩����
                TryMovePlayer(DirectionType.DOWN);
            }
            // ����󂪉����ꂽ�ꍇ
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // �v���C���[�����Ɉړ��ł��邩����
                TryMovePlayer(DirectionType.LEFT);
            }
            //Z�L�[�������ꂽ��
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                //�V�[���̃����[�h
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    /// <summary>
    /// <para>TryMovePlayer</para>
    /// <para>�w�肳�ꂽ�����Ƀv���C���[���ړ��ł��邩���؂���B�ړ��ł���ꍇ�͈ړ�����</para>
    /// </summary>
    /// <param name="direction">�㉺���E�̓���</param>
    private void TryMovePlayer(DirectionType direction)
    {
        // �v���C���[�̌��ݒn���擾
        Vector2Int currentPlayerPos = _gameObjectPosTable[_playerObj];

        // �v���C���[�̈ړ���̈ʒu���v�Z
        Vector2Int nextPlayerPos = GetNextPositionAlong(currentPlayerPos, direction);

        // �v���C���[�̈ړ��悪�X�e�[�W���ł͂Ȃ��ꍇ�͖���
        if (!IsValidPosition(nextPlayerPos))
        {
            return;
        }

        // �v���C���[�̈ړ���Ƀu���b�N�����݂���ꍇ
        if (IsBlock(nextPlayerPos))
        {
            // �u���b�N�̈ړ���̈ʒu���v�Z
            Vector2Int nextBlockPos = GetNextPositionAlong(nextPlayerPos, direction);

            // �u���b�N�̈ړ��悪�X�e�[�W���̏ꍇ���u���b�N�̈ړ���Ƀu���b�N�����݂��Ȃ��ꍇ
            if (IsValidPosition(nextBlockPos) && !IsBlock(nextBlockPos))
            {
                // �ړ�����u���b�N���擾
                GameObject block = GetGameObjectAtPosition(nextPlayerPos);

                // �v���C���[�̈ړ���̃^�C���̏����X�V
                UpdateGameObjectPosition(nextPlayerPos);

                // �u���b�N���ړ�
                block.transform.position = GetDisplayPosition(nextBlockPos.x, nextBlockPos.y);

                // �u���b�N�̈ʒu���X�V
                _gameObjectPosTable[block] = nextBlockPos;

                // �u���b�N�̈ړ���̔ԍ����X�V
                if (_tiles[nextBlockPos.x, nextBlockPos.y] == tileType.GROUND)
                {
                    // �ړ��悪�n�ʂȂ�u���b�N�̔ԍ��ɍX�V
                    _tiles[nextBlockPos.x, nextBlockPos.y] = tileType.BLOCK;
                }
                // �u���b�N�̈ړ��悪�^�[�Q�b�g�Ȃ�
                else if (_tiles[nextBlockPos.x, nextBlockPos.y] == tileType.TARGET)
                {
                    // �ړ��悪�ړI�n�Ȃ�u���b�N�i�ړI�n�̏�j�̔ԍ��ɍX�V
                    _tiles[nextBlockPos.x, nextBlockPos.y] = tileType.BLOCK_ON_TARGET;
                }

                // �v���C���[�̌��ݒn�̃^�C���̏����X�V
                UpdateGameObjectPosition(currentPlayerPos);

                // �v���C���[���ړ�
                _playerObj.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

                // �v���C���[�̈ʒu���X�V
                _gameObjectPosTable[_playerObj] = nextPlayerPos;

                // �v���C���[�̈ړ���̔ԍ����X�V
                if (_tiles[nextPlayerPos.x, nextPlayerPos.y] == tileType.GROUND)
                {
                    // �ړ��悪�n�ʂȂ�v���C���[�̔ԍ��ɍX�V
                    _tiles[nextPlayerPos.x, nextPlayerPos.y] = tileType.PLAYER;
                }
                else if (_tiles[nextPlayerPos.x, nextPlayerPos.y] == tileType.TARGET)
                {
                    // �ړ��悪�ړI�n�Ȃ�v���C���[�i�ړI�n�̏�j�̔ԍ��ɍX�V
                    _tiles[nextPlayerPos.x, nextPlayerPos.y] = tileType.PLAYER_ON_TARGET;
                }
            }
        }
        // �����̓v���C���[�̈ړ���Ƀu���b�N�����݂��Ȃ��ꍇ
        else
        {

            // �v���C���[�̌��ݒn�̃^�C���̏����X�V
            UpdateGameObjectPosition(currentPlayerPos);

            // �v���C���[���ړ�
            _playerObj.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

            // �v���C���[�̈ʒu���X�V
            _gameObjectPosTable[_playerObj] = nextPlayerPos;

            // �v���C���[�̈ړ���̔ԍ����X�V
            if (_tiles[nextPlayerPos.x, nextPlayerPos.y] == tileType.GROUND)
            {
                // �ړ��悪�n�ʂȂ�v���C���[�̔ԍ��ɍX�V
                _tiles[nextPlayerPos.x, nextPlayerPos.y] = tileType.PLAYER;
            }
            // �v���C���[�̈ړ��悪�^�[�Q�b�g�Ȃ�
            else if (_tiles[nextPlayerPos.x, nextPlayerPos.y] == tileType.TARGET)
            {
                // �ړ��悪�ړI�n�Ȃ�v���C���[�i�ړI�n�̏�j�̔ԍ��ɍX�V
                _tiles[nextPlayerPos.x, nextPlayerPos.y] = tileType.PLAYER_ON_TARGET;
            }
        }

        // �Q�[�����N���A�������ǂ����m�F
        CheckCompletion();
    }

    // �����̎��
    private enum DirectionType
    {
        UP, // ��
        RIGHT, // �E
        DOWN, // ��
        LEFT, // ��
    }

    /// <summary>
    /// <para>GetNextPositionAlong</para>
    /// <para>�w�肳�ꂽ�����̈ʒu��Ԃ�</para>
    /// </summary>
    /// <param name="pos">�v���C���[�̍��W</param>
    /// <param name="direction">�㉺���E�̓���</param>
    /// <returns>�ړ���̍��W</returns>
    private Vector2Int GetNextPositionAlong(Vector2Int pos, DirectionType direction)
    {
        switch (direction)
        {
            // ��
            case DirectionType.UP:

                //�z��̎Q�Ƃ���s��-1
                pos.y -= 1;
                //�ړ���������������
                _playerObj.transform.eulerAngles = new Vector3Int(0, 0, 0);
                break;

            // �E
            case DirectionType.RIGHT:

                //�z��̎Q�Ƃ�����-1
                pos.x += 1;
                //�ړ���������������
                _playerObj.transform.eulerAngles = new Vector3Int(0, 0, 270);
                break;

            // ��
            case DirectionType.DOWN:
                //�z��̎Q�Ƃ���s��+1
                pos.y += 1;
                //�ړ���������������
                _playerObj.transform.eulerAngles = new Vector3Int(0, 0, 180);
                break;

            // ��
            case DirectionType.LEFT:
                //�z��̎Q�Ƃ�����+1
                pos.x -= 1;
                //�ړ���������������
                _playerObj.transform.eulerAngles = new Vector3Int(0, 0, 90);
                break;
        }
        return pos;
    }

    /// <summary>
    /// <para>UpdateGameObjectPosition</para>
    /// <para>�w�肳�ꂽ�ʒu�̃^�C�����X�V</para>
    /// </summary>
    /// <param name="pos">�X�V������W</param>
    private void UpdateGameObjectPosition(Vector2Int pos)
    {
        // �w�肳�ꂽ�ʒu�̃^�C���̔ԍ����擾
        tileType cell = _tiles[pos.x, pos.y];

        // �v���C���[�������̓u���b�N�̏ꍇ
        if (cell == tileType.PLAYER || cell == tileType.BLOCK)
        {
            // �n�ʂɕύX
            _tiles[pos.x, pos.y] = tileType.GROUND;
        }
        // �ړI�n�ɏ���Ă���v���C���[�������̓u���b�N�̏ꍇ
        else if (cell == tileType.PLAYER_ON_TARGET || cell == tileType.BLOCK_ON_TARGET)
        {
            // �ړI�n�ɕύX
            _tiles[pos.x, pos.y] = tileType.TARGET;
        }
    }

    /// <summary>
    /// <para>CheckCompletion</para>
    /// <para>�Q�[�����N���A�������ǂ����m�F</para>
    /// </summary>
    private void CheckCompletion()
    {
        // �ړI�n�ɏ���Ă���u���b�N�̐����v�Z
        int blockOnTargetCount = 0;

        //�s���Q��
        for (int y = 0; y < _stageRows; y++)
        {
            //����Q��
            for (int x = 0; x < _stageColumns; x++)
            {
                //���̍��W���u�u���b�N�̏�����^�[�Q�b�g�v�Ȃ�
                if (_tiles[x, y] == tileType.BLOCK_ON_TARGET)
                {
                    //�u�^�[�Q�b�g�ɏ�����u���b�N�v�̐������Z
                    blockOnTargetCount++;
                }
            }
        }

        // ���ׂẴu���b�N���ړI�n�̏�ɏ���Ă���ꍇ
        if (blockOnTargetCount == _blockCount)
        {
            // �Q�[���N���A
            _isClear = true;
        }
    }


}

