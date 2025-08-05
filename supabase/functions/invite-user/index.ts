import { serve } from 'https://deno.land/std@0.168.0/http/server.ts'
import { createClient } from 'https://esm.sh/@supabase/supabase-js@2.0.0'
import { decode } from "https://deno.land/x/djwt@v2.8/mod.ts";
import { corsHeaders } from '../_shared/cors.ts'

console.log(`Function "invite-user" up and running!`)

serve(async (req) => {
  if (req.method === 'OPTIONS') {
    return new Response('ok', { headers: corsHeaders })
  }

  try {
    // 1. Authorization header'ını al ve JWT'yi çıkar
    const authHeader = req.headers.get('Authorization')
    if (!authHeader) {
      throw new Error('Missing Authorization header.')
    }
    const jwt = authHeader.replace('Bearer ', '')
    const [header, payload, signature] = decode(jwt)

    // 2. JWT'nin içindeki rolleri kontrol et
    const roles = (payload as any).app_metadata?.roles || []
    if (!roles.includes('Admin')) {
      throw new Error('Not authorized: Admin role required.')
    }

    // 3. Rol kontrolü başarılıysa, admin istemcisini oluştur
    const supabaseAdmin = createClient(
      Deno.env.get('SUPABASE_URL') ?? '',
      Deno.env.get('SUPABASE_SERVICE_ROLE_KEY') ?? ''
    )

    // 4. Body'den e-postayı ve ek kullanıcı verilerini al
    const { email, data: userData } = await req.json();
    if (!email) {
      throw new Error('Email is required.');
    }

    console.log('Received user data for invitation:', userData);

    // 5. Adım 1: Kullanıcıyı sadece user_metadata ile davet et
    const { data: inviteData, error: inviteError } = await supabaseAdmin.auth.admin.inviteUserByEmail(email, {
      data: userData, // Bu, user_metadata'ya gider (first_name, last_name vb.)
    });

    if (inviteError) {
      console.error('Error during inviteUserByEmail:', inviteError);
      throw new Error(`Supabase invite error: ${inviteError.message}`);
    }
    if (!inviteData || !inviteData.user) {
      throw new Error('Failed to invite user or get user data back.');
    }

    const userId = inviteData.user.id;
    console.log(`User invited successfully with ID: ${userId}`);

    // 6. Adım 2: Davet edilen kullanıcının app_metadata'sını (roller ve durum) güncelle
    const { data: updateData, error: updateError } = await supabaseAdmin.auth.admin.updateUserById(
      userId,
      {
        app_metadata: {
          roles: ['Customer'],
          status: 'invited'
        }
      }
    );

    if (updateError) {
      console.error('Error during updateUserById:', updateError);
      throw new Error(`Supabase metadata update error: ${updateError.message}`);
    }

    console.log('User metadata updated successfully.');

    return new Response(JSON.stringify({ data: updateData }), {
      headers: { ...corsHeaders, 'Content-Type': 'application/json' },
      status: 200,
    });
  } catch (error) {
    console.error('Overall error in invite-user function:', error);
    return new Response(JSON.stringify({ error: error.message }), {
      headers: { ...corsHeaders, 'Content-Type': 'application/json' },
      status: 500, // Daha genel bir sunucu hatası kodu kullanalım
    });
  }
})
