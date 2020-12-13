using HintKeep.Services.Implementations.Configs;

namespace HintKeep.Services.Implementations
{
    public class SaltService : ISaltService
    {
        private readonly SaltServiceConfig _config;
        private readonly IRngService _rngService;

        public SaltService(SaltServiceConfig config, IRngService rngService)
            => (_config, _rngService) = (config, rngService);

        public string GetSalt()
            => _rngService.Generate(_config.SaltLength);
    }
}