namespace blackbox.utils.baerer {
    public static class JwtProvider {
        public static string Provide(string scope) {
            var handler = new JwtSignInHandler();
            var principal = new System.Security.Claims.ClaimsPrincipal (new [] {
                new System.Security.Claims.ClaimsIdentity (new [] {
                    new System.Security.Claims.Claim ("scope", scope)
                })
            });
            return handler.BuildJwt(principal); 
        }
    }
}