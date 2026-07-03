using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class JuiceManager : Singleton<JuiceManager>
{
    #region CONFIGURATION
    [Header("_________________________________________________________")]
    [Header("VFX data")]
    [SerializeField] private GameObject _resourceVFX;
    [SerializeField] private Material _goldMat;
    [SerializeField] private Material _srMat;
    [SerializeField] private Material _claimMat;
    [SerializeField] private Material _carnivalistMat;
    [SerializeField] private Material _scoreMat;
    [SerializeField] private GameObject _spawnUnitVFX;
    [SerializeField] private GameObject _dustInfraVFX;
    [Header("_________________________________________________________")]
    [Header("UI VFX data")]
    [SerializeField] private Camera _renderTextureCam;
    [SerializeField] private GameObject _endGameCurtainVFX;
    [SerializeField] private GameObject _endGameConfettiVFX;
    [SerializeField] private float _endGameFirstConfettiTilt = 15f;
    [SerializeField] private GameObject _endGameFireworkVFX;
    [SerializeField] private float _endGameFireworkPosMaxOffset;
    [SerializeField] private int _endGameFireworkQuantity = 3;
    [SerializeField] private GameObject _resourceVFXforUI;
    [Header("_________________________________________________________")]
    [Header("Combo Visualization")]
    [SerializeField] private GameObject _comboVFX;
    [SerializeField] private Vector3 _spawnOffset = new Vector3(0,.2f,0);
    [SerializeField] private float _minComboDuration = 1.0f;
    [SerializeField] private float _maxComboDuration = 3.0f;
    [SerializeField] private float _durationJitter = 0.1f;
    [SerializeField] private float _maxDurationDistance = 10f;
    [SerializeField] float _arcHeightPerUnit = 0.15f;
    [SerializeField] float _arcMinHeight = 1f;
    [Header("_________________________________________________________")]
    [Header("Wave VFX")]
    [SerializeField] float _waveRingDelay = 0.25f;
    #endregion

    private Color WHITE = new Color(1f, 1f, 1f, 1f);

    private GameObject _popUpVisualizingCombo;
    private bool _playingIncomingComboVFX;
    private bool _playingOutgoingComboVFX;
    private Dictionary<GameObject, ArcMoveData> _incomingVFX = new Dictionary<GameObject, ArcMoveData>();
    private Dictionary<GameObject, ArcMoveData> _outgoingVFX = new Dictionary<GameObject, ArcMoveData>();

    private Tile _waveSourceTile;
    private List<(Tile tile, int points, bool isGain)> _pendingWaveVFX = new();
    private Coroutine _waveCoroutine;

    public GameObject PopUpVisualizingCombo { get => _popUpVisualizingCombo; set => _popUpVisualizingCombo = value; }

    protected override void OnAwake()
    {
        ExplorationManager.Instance.OnScoutSpawned += scout => SpawnUnitVFX(scout.CurrentTile);

        EntertainmentManager.Instance.OnEntertainmentSpawned += EntertainmentSpawned;
        EntertainmentManager.Instance.OnEntertainmentRemoved += (data, tile) => EntertainmentRemoved(tile);
        EntertainmentManager.Instance.OnScoreGained += (tile, value) => BufferWaveVFX(tile, value, true);
        EntertainmentManager.Instance.OnScoreLost += (tile, value) => BufferWaveVFX(tile, value, false);

        ResourcesManager.Instance.OnGoldGained += (tile, value) => ResourceGain(tile, value, ExtendedResource.Gold);
        ResourcesManager.Instance.OnGoldSpent += (value) => PlayUIResourceVFX(value, ExtendedResource.Gold, UIManager.Instance.VfxAnchorGold, Transaction.Spent);
        ResourcesManager.Instance.OnSpecialResourcesGained += (tile, value) => ResourceGain(tile, value, ExtendedResource.SpecialResources);
        ResourcesManager.Instance.OnSpecialResourcesSpent += (value) => PlayUIResourceVFX(value, ExtendedResource.SpecialResources, UIManager.Instance.VfxAnchorSR, Transaction.Spent);
        ResourcesManager.Instance.OnClaimGained += (tile, value) => ResourceGain(tile, value, ExtendedResource.Claim);
        ResourcesManager.Instance.OnClaimSpent += (value) => PlayUIResourceVFX(value, ExtendedResource.Claim, UIManager.Instance.VfxAnchorClaim, Transaction.Spent);
        ResourcesManager.Instance.OnCarnivalistGained += (tile, value) => ResourceGain(tile, value, ExtendedResource.Carnivalist);
        ResourcesManager.Instance.OnCarnivalistSpent += (tile, value) => CarnivalistSpent(tile, value);

        GameManager.Instance.OnGameFinished += EndGameVFX;

        //ExploitationManager.Instance.OnInfraBuilded += tile => DustVFX(tile); Not convinced by the visual effect of this
        //ExploitationManager.Instance.OnInfraDestroyed += tile => DustVFX(tile);
    }

    #region GAMEPLAY VFX
    private void ResourceGain(Tile tile, int value, ExtendedResource resource)
    {
        switch (resource)
        {
            case ExtendedResource.Gold:
                if (tile)
                    PlayResourceVFX(tile, value, _goldMat);
                else
                    PlayUIResourceVFX(value, ExtendedResource.Gold, UIManager.Instance.VfxAnchorGold, Transaction.Gain);
                break;
            case ExtendedResource.SpecialResources:
                if (tile)
                    PlayResourceVFX(tile, value, _srMat);
                else
                    PlayUIResourceVFX(value, ExtendedResource.SpecialResources, UIManager.Instance.VfxAnchorSR, Transaction.Gain);
                break;
            case ExtendedResource.Claim:
                if (tile)
                    PlayResourceVFX(tile, value, _claimMat);
                else
                    PlayUIResourceVFX(value, ExtendedResource.Claim, UIManager.Instance.VfxAnchorClaim, Transaction.Gain);
                break;
            case ExtendedResource.Carnivalist:
                if (tile)
                    PlayResourceVFX(tile, value, _carnivalistMat);
                else
                    PlayUIResourceVFX(value, ExtendedResource.Carnivalist, UIManager.Instance.VfxAnchorCarnivalist, Transaction.Gain);
                break;
        }
    }

    private void SpawnUnitVFX(Tile tile)
    {
        if (EntertainmentManager.Instance.IsPredictingPoints)
            return;
        Instantiate(_spawnUnitVFX, _spawnUnitVFX.transform.position + tile.transform.position, _spawnUnitVFX.transform.rotation);
    }

    private void EntertainmentSpawned(Entertainment ent)
    {
        BeginWaveFrom(ent.Tile);
        SpawnUnitVFX(ent.Tile);
    }

    private void EntertainmentRemoved(Tile tile)
    {
        BeginWaveFrom(tile);
    }

    private void PlayResourceVFX(Tile tile, int value, Material mat, Color color = default)
    {
        if (color == default)
            color = WHITE;
        GameObject vfx = GameObject.Instantiate(_resourceVFX, _resourceVFX.transform.position + tile.transform.position, _resourceVFX.transform.rotation);

        ParticleSystem particleSystem = vfx.GetComponent<ParticleSystem>();

        particleSystem.emission.SetBurst(0, new ParticleSystem.Burst(0, value));

        vfx.GetComponent<ParticleSystemRenderer>().material = mat;

        ParticleSystem.MainModule main = particleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color);

        particleSystem.Play();
    }

    private void CarnivalistSpent(Tile tile, int value)
    {
        if (tile)
            PlayResourceVFX(tile, value, _carnivalistMat, UIManager.Instance.ColorCantAfford);
        else
            PlayUIResourceVFX(value, ExtendedResource.Carnivalist, UIManager.Instance.VfxAnchorCarnivalist, Transaction.Spent);
    }

    private void DustVFX(Tile tile)
    {
        Instantiate(_dustInfraVFX, _dustInfraVFX.transform.position + tile.transform.position, _dustInfraVFX.transform.rotation);
    }
    #endregion

    #region UI VFX
    private void EndGameVFX()
    {
        Instantiate(_endGameCurtainVFX, PlaceAtViewport(UIManager.Instance.VfxAnchorEndCurtain), _endGameCurtainVFX.transform.rotation);
    }

    public void EndGameEndCountingVFX()
    {
        Instantiate(_endGameConfettiVFX, PlaceAtViewport(UIManager.Instance.VfxAnchorEndConfetti1), _endGameConfettiVFX.transform.rotation * Quaternion.Euler(0f, 0f, _endGameFirstConfettiTilt));
        Instantiate(_endGameConfettiVFX, PlaceAtViewport(UIManager.Instance.VfxAnchorEndConfetti2), _endGameConfettiVFX.transform.rotation);

        for (int i = 0; i < _endGameFireworkQuantity; i++)
        {
            Instantiate(_endGameFireworkVFX, PlaceAtViewport(UIManager.Instance.VfxAnchorEndFirework1, _endGameFireworkPosMaxOffset), _endGameFireworkVFX.transform.rotation);
            Instantiate(_endGameFireworkVFX, PlaceAtViewport(UIManager.Instance.VfxAnchorEndFirework2, _endGameFireworkPosMaxOffset), _endGameFireworkVFX.transform.rotation);
        }
    }

    private Vector3 PlaceAtViewport(RectTransform uiElement, float maxOffset = 0f)
    {
        // convert UI position to viewport
        Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(null, uiElement.position);
        Vector2 targetPos = new Vector2(screenPt.x / Screen.width, screenPt.y / Screen.height);
        Vector3 vp = new Vector3(targetPos.x, targetPos.y, 1f);

        // base world‐space position
        Vector3 worldPos = _renderTextureCam.ViewportToWorldPoint(vp);

        // random offset between 0 and maxOffset on X/Y axes
        float dx = Random.Range(0f, maxOffset);
        float dy = Random.Range(0f, maxOffset);

        return worldPos + new Vector3(dx, dy, 0f);
    }

    private void PlayUIResourceVFX(int value, ExtendedResource resource, RectTransform uiElement, Transaction transaction)
    {
        GameObject vfx = Instantiate(_resourceVFXforUI, PlaceAtViewport(uiElement), _resourceVFXforUI.transform.rotation);

        TextMeshPro text = vfx.GetComponentInChildren<TextMeshPro>();
        text.text = $"{(transaction == Transaction.Gain ? "+" : "-")}{value}";
        switch (resource)
        {
            case ExtendedResource.Gold:
                text.text += "<sprite name=\"Gold_Emoji\">";
                break;
            case ExtendedResource.SpecialResources:
                text.text += "<sprite name=\"SR_Emoji\">";
                break;
            case ExtendedResource.Claim:
                text.text += "<sprite name=\"Claim_Emoji\">";
                break;
            case ExtendedResource.Carnivalist:
                text.text += "<sprite name=\"Carnivalist_Emoji\">";
                break;
        }
        vfx.SetActive(true);
        string trigger = transaction == Transaction.Gain ? "Gain" : "Spent";
        vfx.GetComponent<Animator>().SetTrigger(trigger);
    }
    #endregion

    #region VISUALIZING COMBO
    private void Update()
    {
        if (_playingIncomingComboVFX)
        {
            _playingIncomingComboVFX = false;

            foreach (var key in _incomingVFX.Keys.ToList())
            {
                if (key.activeSelf)
                {
                    _playingIncomingComboVFX = true;

                    var data = _incomingVFX[key];
                    bool arrived = MoveWithArc(key, ref data);
                    _incomingVFX[key] = data;

                    if (arrived)
                        key.SetActive(false);
                }
            }

            if (!_playingIncomingComboVFX)
            {
                _playingOutgoingComboVFX = true;
                foreach (var key in _outgoingVFX.Keys)
                    key.SetActive(true);
            }
        }
        else if (_playingOutgoingComboVFX)
        {
            _playingOutgoingComboVFX = false;

            foreach (var key in _outgoingVFX.Keys.ToList())
            {
                if (key.activeSelf)
                {
                    _playingOutgoingComboVFX = true;

                    var data = _outgoingVFX[key];
                    bool arrived = MoveWithArc(key, ref data);
                    _outgoingVFX[key] = data;

                    if (arrived)
                        key.SetActive(false);
                }
            }
        }
        else
        {
            StartComboAnimation(true);
        }
    }

    private bool MoveWithArc(GameObject go, ref ArcMoveData data)
    {
        VectorPair positions = data.positions;

        Vector3 axis = positions.to - positions.from;
        float totalDist = axis.magnitude;
        if (totalDist <= Mathf.Epsilon)
        {
            go.transform.position = positions.to;
            return true;
        }

        Vector3 axisDir = axis / totalDist;

        data.elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(data.elapsed / data.duration);

        Vector3 flat = positions.from + axisDir * (t * totalDist);

        float arcHeight = Mathf.Max(_arcMinHeight, _arcHeightPerUnit * totalDist);
        flat.y = Mathf.Lerp(positions.from.y, positions.to.y, t)
                 + arcHeight * Mathf.Sin(Mathf.PI * t);

        go.transform.position = flat;

        if (t >= 0.999f)
        {
            go.transform.position = positions.to;
            return true;
        }

        return false;
    }

    private float GetDurationForArc(ArcMoveData data)
    {
        float dist = (data.positions.to - data.positions.from).magnitude;
        float t = Mathf.InverseLerp(0f, _maxDurationDistance, dist);
        float baseDuration = Mathf.Lerp(_minComboDuration, _maxComboDuration, t);
        float dur = baseDuration + Random.Range(-_durationJitter, _durationJitter);
        return Mathf.Max(0.01f, dur);
    }

    private void StartComboAnimation(bool resetPos = false)
    {
        if (resetPos)
        {
            foreach (var kvp in _incomingVFX)
                kvp.Key.transform.position = kvp.Value.positions.from;

            foreach (var kvp in _outgoingVFX)
                kvp.Key.transform.position = kvp.Value.positions.from;
        }

        if (_incomingVFX.Count == 0 && _outgoingVFX.Count == 0)
            return;

        foreach (var key in _incomingVFX.Keys.ToList())
        {
            var data = _incomingVFX[key];
            data.elapsed = 0f;
            data.duration = GetDurationForArc(data);
            _incomingVFX[key] = data;
            key.SetActive(false);
        }

        foreach (var key in _outgoingVFX.Keys.ToList())
        {
            var data = _outgoingVFX[key];
            data.elapsed = 0f;
            data.duration = GetDurationForArc(data);
            _outgoingVFX[key] = data;
            key.SetActive(false);
        }

        if (_incomingVFX.Count != 0)
        {
            _playingIncomingComboVFX = true;
            _playingOutgoingComboVFX = false;

            foreach (var key in _incomingVFX.Keys)
                key.SetActive(true);
        }
        else
        {
            _playingIncomingComboVFX = false;
            _playingOutgoingComboVFX = true;

            foreach (var key in _outgoingVFX.Keys)
                key.SetActive(true);
        }
    }

    public void KillAllComboVFX()
    {
        _popUpVisualizingCombo = null;
        foreach (var vfx in _incomingVFX)
        {
            Destroy(vfx.Key);
        }
        foreach (var vfx in _outgoingVFX)
        {
            Destroy(vfx.Key);
        }
        _incomingVFX.Clear();
        _outgoingVFX.Clear();
    }

    private void CreateEntertainmentComboVFX(Tile fromTile, Tile toTile, int points, Dictionary<GameObject, ArcMoveData> direction)
    {
        GameObject vfx = Instantiate(_comboVFX, fromTile.transform.position + _spawnOffset, CameraManager.Instance.transform.rotation);
        TextMeshPro text = vfx.GetComponentInChildren<TextMeshPro>();
        text.text = "+" + points.ToString() + "<sprite name=\"Point_Emoji\">";
        direction.Add(vfx, new ArcMoveData
        {
            positions = new VectorPair(
                vfx.transform.position,
                toTile.transform.position + _spawnOffset
            ),
            elapsed = 0f,
            duration = 0f
        });
    }

    private void CreateExploitationComboVFX(Tile fromTile, Tile toTile, string textContent, Dictionary<GameObject, ArcMoveData> direction)
    {
        GameObject vfx = Instantiate(_comboVFX, fromTile.transform.position + _spawnOffset, CameraManager.Instance.transform.rotation);
        TextMeshPro textObject = vfx.GetComponentInChildren<TextMeshPro>();
        textObject.text = textContent;
        direction.Add(vfx, new ArcMoveData
        {
            positions = new VectorPair(
                vfx.transform.position,
                toTile.transform.position + _spawnOffset
            ),
            elapsed = 0f,
            duration = 0f
        });
    }

    public bool VisualizeExploitationCombo(
        Dictionary<Tile, List<ResourceToIntMap>> internalIncomeSources, Dictionary<Tile, List<ResourceToIntMap>> externalIncomeSources,
        Dictionary<Tile, int> internalCarnivalistsSources,
        Dictionary<Tile, List<ResourceToIntMap>> impactedTilesIncomes, Dictionary<Tile, int> impactedTilesCarnivalists,
        Dictionary<Tile, int> entImpactedByTile, Dictionary<Tile, int> tilesBoostingEnt,
        Tile refTile)
    {
        if (internalIncomeSources.Count != 0 || externalIncomeSources.Count != 0 
            || internalCarnivalistsSources.Count != 0 || tilesBoostingEnt.Count != 0)
        {
            Dictionary<Tile, List<ResourceToIntMap>> incomeSources = new Dictionary<Tile, List<ResourceToIntMap>>();
            foreach (var kvp in internalIncomeSources)
            {
                if (kvp.Key == refTile)
                    continue;
                if (incomeSources.ContainsKey(kvp.Key))
                    incomeSources[kvp.Key] = Utilities.MergeResourceToIntMaps(incomeSources[kvp.Key], kvp.Value);
                else
                    incomeSources[kvp.Key] = Utilities.CloneResourceToIntMaps(kvp.Value);
            }
            foreach (var kvpBis in externalIncomeSources)
            {
                if (kvpBis.Key == refTile)
                    continue;
                if (incomeSources.ContainsKey(kvpBis.Key))
                    incomeSources[kvpBis.Key] = Utilities.MergeResourceToIntMaps(incomeSources[kvpBis.Key], kvpBis.Value);
                else
                    incomeSources[kvpBis.Key] = Utilities.CloneResourceToIntMaps(kvpBis.Value);
            }
            Dictionary<Tile, string> sourcesToText = new Dictionary<Tile, string>();
            foreach (var kvpTer in incomeSources)
            {
                sourcesToText[kvpTer.Key] = kvpTer.Value.IncomeToString();
            }
            foreach (var kvpQuatro in internalCarnivalistsSources)
            {
                if (kvpQuatro.Key == refTile)
                    continue;
                if (sourcesToText.ContainsKey(kvpQuatro.Key))
                    sourcesToText[kvpQuatro.Key] += $" & {kvpQuatro.Value}<sprite name=\"Carnivalist_Emoji\">";
                else
                    sourcesToText[kvpQuatro.Key] = $"{kvpQuatro.Value}<sprite name=\"Carnivalist_Emoji\">";
            }
            foreach (var kvp in tilesBoostingEnt)
            {
                if (kvp.Key == refTile)
                    continue;
                if (sourcesToText.ContainsKey(kvp.Key))
                    sourcesToText[kvp.Key] += $" & +{kvp.Value}<sprite name=\"Point_Emoji\">";
                else
                    sourcesToText[kvp.Key] = $" +{kvp.Value}<sprite name=\"Point_Emoji\">";
            }
            foreach (var kvp in sourcesToText)
            {
                CreateExploitationComboVFX(kvp.Key, refTile, kvp.Value, _incomingVFX);
            }
        }
        if (impactedTilesIncomes.Count != 0 || impactedTilesCarnivalists.Count != 0 || entImpactedByTile.Count != 0)
        {
            Dictionary<Tile, string> impactToText = new Dictionary<Tile, string>();
            foreach (var kvp in impactedTilesIncomes)
            {
                if (kvp.Key == refTile)
                    continue;
                impactToText[kvp.Key] = kvp.Value.IncomeToString();
            }
            foreach (var kvpBis in impactedTilesCarnivalists)
            {
                if (kvpBis.Key == refTile)
                    continue;
                if (impactToText.ContainsKey(kvpBis.Key))
                    impactToText[kvpBis.Key] += $" & {kvpBis.Value}<sprite name=\"Carnivalist_Emoji\">";
                else
                    impactToText[kvpBis.Key] = $"{kvpBis.Value}<sprite name=\"Carnivalist_Emoji\">";
            }
            foreach (var kvp in entImpactedByTile)
            {
                if (kvp.Key == refTile)
                    continue;
                if (impactToText.ContainsKey(kvp.Key))
                    impactToText[kvp.Key] += $" & +{kvp.Value}<sprite name=\"Point_Emoji\">";
                else
                    impactToText[kvp.Key] = $"+{kvp.Value}<sprite name=\"Point_Emoji\">";
            }
            foreach (var kvp in impactToText)
            {
                CreateExploitationComboVFX(refTile, kvp.Key, kvp.Value, _outgoingVFX);
            }
        }
        StartComboAnimation();
        return (_incomingVFX.Count != 0 || _outgoingVFX.Count != 0);
    }


    // Visualize the entertainment combo when there is a entertainment placed on refTile
    public bool VisualizeEntertainmentCombo(
        Dictionary<Tile, int> internalSources, Dictionary<Tile, int> externalSources, 
        Dictionary<Tile, int> entImpactedByEnt, Dictionary<Tile, int> entImpactedByTile,
        Tile refTile)
    {
        if (internalSources.Count != 0 || externalSources.Count != 0)
        {
            Dictionary<Tile, int> pointsSources = new Dictionary<Tile, int>(internalSources);
            foreach (var kvp in externalSources)
            {
                if (kvp.Key == refTile)
                    continue;
                if (pointsSources.ContainsKey(kvp.Key))
                    pointsSources[kvp.Key] += kvp.Value;
                else
                    pointsSources[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in pointsSources)
            {
                CreateEntertainmentComboVFX(kvp.Key, refTile, kvp.Value, _incomingVFX);
            }
        }
        if (entImpactedByEnt.Count != 0 || entImpactedByTile.Count != 0)
        {
            Dictionary<Tile, int> pointsGiven = new Dictionary<Tile, int>(entImpactedByEnt);
            foreach (var kvp in entImpactedByTile)
            {
                if (kvp.Key == refTile)
                    continue;
                if (pointsGiven.ContainsKey(kvp.Key))
                    pointsGiven[kvp.Key] += kvp.Value;
                else
                    pointsGiven[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in pointsGiven)
            {
                CreateEntertainmentComboVFX(refTile, kvp.Key, kvp.Value, _outgoingVFX);
            }
        }
        StartComboAnimation();
        return (_incomingVFX.Count != 0 || _outgoingVFX.Count != 0);
    }

    // Visualize the entertainment combo when there is no a entertainment placed on refTile
    public bool VisualizeEntertainmentComboFromTileOnly(Tile refTile)
    {
        foreach (var kvp in refTile.EntImpactedByTile)
        {
            CreateEntertainmentComboVFX(refTile, kvp.Key, kvp.Value, _outgoingVFX);
        }
        StartComboAnimation();
        return (_outgoingVFX.Count != 0);
    }
    #endregion

    #region WAVE VFX
    public void BeginWaveFrom(Tile sourceTile)
    {
        if (EntertainmentManager.Instance.IsPredictingPoints || sourceTile == null)
            return;

        if (_waveSourceTile != null && _waveSourceTile != sourceTile)
            FlushWave();

        _waveSourceTile = sourceTile;
    }

    private void BufferWaveVFX(Tile tile, int points, bool isGain)
    {
        if (EntertainmentManager.Instance.IsPredictingPoints)
            return;
        _pendingWaveVFX.Add((tile, points, isGain));

        // Relance la coroutine à chaque ajout — elle se base sur le snapshot au moment du démarrage
        // On ne relance pas si elle tourne déjà, elle drainera ce qui vient d'arriver
        if (_waveCoroutine == null)
            _waveCoroutine = StartCoroutine(PlayWaveVFX());
    }

    private IEnumerator PlayWaveVFX()
    {
        // On attend la fin de la frame que tout les ent impactés aient rejoint la liste
        yield return new WaitForEndOfFrame();
        // NOTE: Cette coroutine doit rester synchrone jusqu'ici.
        // Si on introduit un yield avant dans SpawnEntertainment ou Initialize,
        // le snapshot sera partiel et le BoostByZoneSize/MergeGroups sera dans
        // la mauvaise wave. Ne pas toucher à cette contrainte sans revoir ce système.

        if (_waveSourceTile == null || _pendingWaveVFX.Count == 0)
        {
            _waveCoroutine = null;
            yield break;
        }

        // BFS depuis la source pour attribuer un ring à chaque tile affectée
        Dictionary<Tile, int> ringByTile = ComputeRings(_waveSourceTile, _pendingWaveVFX);

        // Grouper par ring, ne garder que les rings non vides
        var ringGroups = new SortedDictionary<int, List<(Tile tile, int points, bool isGain)>>();
        foreach (var entry in _pendingWaveVFX)
        {
            int ring = ringByTile.GetValueOrDefault(entry.tile, 0);
            if (!ringGroups.ContainsKey(ring))
                ringGroups[ring] = new List<(Tile, int, bool)>();
            ringGroups[ring].Add(entry);
        }

        bool isFirst = true;
        foreach (var kvp in ringGroups) // SortedDictionary itère dans l'ordre croissant des clés
        {
            if (!isFirst)
                yield return new WaitForSeconds(_waveRingDelay);
            isFirst = false;

            foreach (var (tile, points, isGain) in kvp.Value)
            {
                Color color = isGain ? default(Color) : UIManager.Instance.ColorCantAfford;
                PlayResourceVFX(tile, points, _scoreMat, color);
            }
        }

        _pendingWaveVFX.Clear();
        _waveSourceTile = null;
        _waveCoroutine = null;
    }

    private Dictionary<Tile, int> ComputeRings(Tile source, List<(Tile tile, int points, bool isGain)> targets)
    {
        var targetSet = new HashSet<Tile>(targets.Select(t => t.tile));
        var result = new Dictionary<Tile, int>();
        var visited = new HashSet<Tile> { source };
        var queue = new Queue<(Tile tile, int ring)>();
        queue.Enqueue((source, 0));

        while (queue.Count > 0 && result.Count < targetSet.Count)
        {
            var (current, ring) = queue.Dequeue();

            if (targetSet.Contains(current))
                result[current] = ring;

            foreach (Tile neighbor in current.Neighbors)
            {
                if (neighbor == null || visited.Contains(neighbor))
                    continue;
                visited.Add(neighbor);
                queue.Enqueue((neighbor, ring + 1));
            }
        }

        // Fallback : tiles non atteintes par le BFS (ne devrait pas arriver)
        foreach (var (tile, _, _) in targets)
            if (!result.ContainsKey(tile))
                result[tile] = 0;

        return result;
    }

    public void FlushWave()
    {
        if (_pendingWaveVFX.Count == 0 && _waveCoroutine == null)
            return;

        if (_waveCoroutine != null)
        {
            StopCoroutine(_waveCoroutine);
            _waveCoroutine = null;
        }

        // Joue tout immédiatement sans délai
        foreach (var (tile, points, isGain) in _pendingWaveVFX)
        {
            Color color = isGain ? default(Color) : UIManager.Instance.ColorCantAfford;
            PlayResourceVFX(tile, points, _scoreMat, color);
        }

        _pendingWaveVFX.Clear();
        _waveSourceTile = null;
    }
    #endregion
}
