using System;
using UnityEngine;


/// <summary>
/// VERY primitive animator example.
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Animator _anim;

    [SerializeField] private SpriteRenderer _sprite;

    [Header("Settings")]
    [SerializeField, Range(1f, 3f)]
    private float _maxIdleSpeed = 2;

    [SerializeField] private float _maxTilt = 5;
    [SerializeField] private float _tiltSpeed = 20;
    private float _lastDirectionX;


    [Header("Particles")][SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private ParticleSystem _launchParticles;
    [SerializeField] private ParticleSystem _moveParticles;
    [SerializeField] private ParticleSystem _landParticles;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip[] _footsteps;

    private AudioSource _source;
    private IPlayerController _player;
    private bool _grounded;
    private ParticleSystem.MinMaxGradient _currentGradient;
    private bool isFacingRight = true;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _player = GetComponentInParent<IPlayerController>();
    }

    private void OnEnable()
    {
        _player.Jumped += OnJumped;
        _player.GroundedChanged += OnGroundedChanged;
        _player.Attacked += OnAttack;

        _moveParticles.Play();
    }

    private void OnDisable()
    {
        _player.Jumped -= OnJumped;
        _player.GroundedChanged -= OnGroundedChanged;
        _player.Attacked -= OnAttack;

        _moveParticles.Stop();
    }

    private void Update()
    {

        if (_player == null) return;

        DetectGroundColor();

        HandleSpriteFlip();

        HandleIdleSpeed();

        HandleCharacterTilt();

    }
    private void HandleSpriteFlip()
    {
        float currentX = _player.FrameInput.x;

        // Rotate character instead of flipping sprite
        if ((isFacingRight && currentX < 0) || (!isFacingRight && currentX > 0))
        {
            // Face right (0 degrees) or left (180 degrees) on Y-axis
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }

        // Detect direction change, only when grounded and there's horizontal input
        if (currentX != 0 && Mathf.Sign(currentX) != Mathf.Sign(_lastDirectionX) && _grounded)
        {
            _moveParticles.Play();
        }

        // Update last known movement direction only when moving
        if (currentX != 0)
            _lastDirectionX = currentX;
    }


    private void HandleIdleSpeed()
    {
        var inputStrength = Mathf.Abs(_player.FrameInput.x);
        //Debug.Log($"FrameInput.x: {_player.FrameInput.x}, inputStrength: {Mathf.Abs(_player.FrameInput.x)}");

        _anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, inputStrength));
        //Debug.Log($"IdleSpeed: {Mathf.Lerp(1, _maxIdleSpeed, inputStrength)}");
        //_moveParticles.transform.localScale = Vector3.MoveTowards(_moveParticles.transform.localScale, Vector3.one * inputStrength, 2 * Time.deltaTime);
        //added this to stop the particles when its not moving left or right and its grounded:
        // if (inputStrength == 0 && _grounded)
        // {
        //     _moveParticles.Stop();
        // }


    }

    private void HandleCharacterTilt()
    {
        var runningTilt = _grounded ? Quaternion.Euler(0, 0, _maxTilt * _player.FrameInput.x) : Quaternion.identity;
        _anim.transform.up = Vector3.RotateTowards(_anim.transform.up, runningTilt * Vector2.up, _tiltSpeed * Time.deltaTime, 0f);
    }

    private void OnJumped()
    {
        Debug.Log("OnJumped() was triggered");

        _anim.SetTrigger(JumpKey);
        _anim.ResetTrigger(GroundedKey);
        _jumpParticles.Play();

        if (_grounded) // Avoid coyote
        {
            SetColor(_jumpParticles);
            SetColor(_launchParticles);

        }
    }

    private void OnGroundedChanged(bool grounded, float impact)
    {
        _grounded = grounded;

        if (grounded)
        {
            DetectGroundColor();
            //SetColor(_landParticles);

            _anim.SetTrigger(GroundedKey);
            if (_footsteps != null && _footsteps.Length > 0)
            {
                _source.PlayOneShot(_footsteps[UnityEngine.Random.Range(0, _footsteps.Length)]);
            }
            else
            {
                Debug.LogWarning("Footsteps audio clips not assigned or empty!");
            }

            _moveParticles.Stop();

            //_landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
            //_landParticles.Play();
        }
        else
        {
            //_moveParticles.Stop();
        }
    }
    private void OnAttack()
    {
        if (_anim != null)
    {
        _anim.SetTrigger(AttackKey);
        // Optionally play attack particles or sounds here if you have them
        // e.g. _attackParticles.Play();
        // e.g. _source.PlayOneShot(_attackSound);
    }
    }
    private void DetectGroundColor()
    {
        var hit = Physics2D.Raycast(transform.position, Vector3.down, 2);

        if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) return;
        var color = r.color;
        _currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
        SetColor(_moveParticles);
    }

    private void SetColor(ParticleSystem ps)
    {
        var main = ps.main;
        main.startColor = _currentGradient;
    }

    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
    private static readonly int JumpKey = Animator.StringToHash("Jump");  
    private static readonly int AttackKey = Animator.StringToHash("Attack");
  
    }
    
