using UnityEngine;
using System.Collections;
using System;
using SimpleJSON;
using UnityEngine.Networking;

using UnityEngine.UI;

public class ExperimentNetworking : NetworkBehaviour
{
	public bool urlReturn;
	string _message;
	public int resultCoins = -1;

	public int effortCoins;
	public CoinManager coinManager;

	//returns from url
	string returnString;
	int returnInt;
	float returnFloat;

	public Text canvasText;

	[SyncVar] public string message = "";
	[SyncVar] public string resultMessage = "";

	void Start ()
	{
		//start at stage 0
		coinManager = (CoinManager)GetComponent<CoinManager> ();
		urlReturn = true;
	} s i n g   U n i t y E n g i n e ; 

	void Update(){
		_message =message;
		if (message != _message) {
			//send update of result Message too for when it comes in
			//empy message not displayed
			coinManager.player.Cmd_broadcast (message, resultMessage);

		}
		_message = message;
	}
	public IEnumerator FetchStage (string _url, string find, string findInt, runState _mode)
	{
		
			urlReturn = false;
			//Debug.LogWarning (url);

			yield return StartCoroutine (WaitForSeconds (.5f));
			WWW www = new WWW (_url);

			yield return StartCoroutine (WaitForRequest (www));
			//go to next step when done
			urlReturn = true;
			// StringBuilder sb = new StringBuilder();
			string result = www.text;
			JSONNode node = JSON.Parse (result);

			if (node != null) {
				try {
					//get stage message
					message = node ["message"];
					if (node ["type_stage"] == "End") {
						resultMessage = message;
					}
				} catch {
					//message = null;
					//yield return false;
				}

				//Debug.Log (message);

				if (find.Length != 0) {

					returnString = node [find];
					returnFloat = 0;
					//	Debug.LogWarning (node);
					if (find == "Results") {
						//hack to get results into message- the time delay
						//mens you cannot pick this up in the state machine


						if (float.TryParse (returnString, out returnFloat)) {
							//get back result from group
							//when get result show it`


							//FIXME
							if (returnFloat > 0 && !message.Equals ("")) {
								//set to display result only
								resultCoins =	coinManager.maxCoins + 1 - effortCoins + (int)returnFloat;
								coinManager.result = true;

								coinManager.currentCoins -= (int)returnFloat;




								//display results - no entered coins show anymore - fixit

								canvasText.text = message + returnFloat.ToString ();
								//	Debug.LogWarning (message + returnFloat.ToString ());
								//stop broadcast
								message="";
								yield return StartCoroutine (WaitForSeconds (.5f));
								//delay display of final message
								//stop broadcast
								message="";
								if(resultMessage!="")canvasText.text = resultMessage + resultCoins.ToString ();
								resultMessage = "";
							}

							yield return true;

							//message for localplayer/tokenbox only
						}

						yield return true;
					} else if (Int32.TryParse (node [findInt], out returnInt)) {

						//Debug.Log(returnInt);
						yield return true;
					}

					yield  return true;
				} else {

					if (Int32.TryParse (node [findInt], out returnInt))
						yield return true;

				}
			} else {
				//Debug.LogWarning ("No node on api read for " + find + " or " + findInt);
				//canvas.message = "Errer in stages for experiment: " + node;
				yield return true;

			}

		yield break;
	}

	IEnumerator setupWait (float num)
	{
		yield return WaitForSeconds (num);


	}

	public IEnumerator WaitForRequest (WWW www)
	{

		yield return www;

	}

	IEnumerator WaitForSeconds (float num)
	{

		yield return new WaitForSeconds (num);

	}
 
 u s i n g   S y s t e m ; 
 
 u s i n g   U n i t y E n g i n e . V R ; 
 
 u s i n g   S i m p l e J S O N ; 
 
 u s i n g   U n i t y E n g i n e . N e t w o r k i n g ; 
 
 u s i n g   S y s t e m . C o l l e c t i o n s . G e n e r i c ; 
 
 u s i n g   U n i t y E n g i n e . U I ; 
 
 
 
 p u b l i c   c l a s s   E x p e r i m e n t C o n t r o l l e r   :   N e t w o r k B e h a v i o u r 
 
 { 
 
 	 / / s e t   i n   p l a y e r n e t w o r k c o n t r o l l e r 
 
 
 
 	 p u b l i c   i n t   p a r t i c i p a n t _ i d ; 
 
 	 p u b l i c   i n t   p a r t i c i p a n t ; 
 
 
 
 	 / / f i n d   i n   g a m e 
 
 	 p u b l i c   T e x t F i l e R e a d e r   t e x t F i l e R e a d e r ; 
 
 
 
 	 / / m o v e   h a n d   t o   e n t e r   c o i n s   -   e f f e c t o r   f r o m   P l a y e r n e t w o r k s e t u p 
 
 	 p u b l i c   T r a n s f o r m   l e f t h a n d E f f e c t o r ; 
 
 	 p u b l i c   b o o l   e n t e r ; 
 
 	 p u b l i c   T r a n s f o r m   b u t t o n ; 
 
 	 [ S y n c V a r ] p u b l i c   b o o l   i k A c t i v e ; 
 
 	 [ S y n c V a r ] p u b l i c   i n t   r o u n d _ i d ; 
 
 	 p u b l i c   f l o a t   t r a n s P o s ; 
 
 	 / / w a t i i n g   f o r   u r l   o u t p u t 
 
 	 b o o l   u r l R e t u r n ; 
 
 	 / / f i r s t   r u n   o f   a n s w e r   e t c 
 
 	 b o o l   u p d a t e   =   t r u e ; 
 
 	 s t r i n g   u r l ; 
 
 	 i n t   r e s u l t C o i n s   =   - 1 ; 
 
 
 
 	 i n t   e f f o r t C o i n s ; 
 
 
 
 	 p u b l i c   e n u m   r u n S t a t e 
 
 	 { 
 
 	 	 s t a r t , 
 
 	 	 w a i t , 
 
 	 	 a s k , 
 
 	 	 a n s w e r , 
 
 	 	 r e s u l t , 
 
 	 	 e n d } 
 
 
 
 	 ; 
 
 
 
 
 
 	 p u b l i c   b o o l   i s H o s t ; 
 
 	 p u b l i c   b o o l   w a i t i n g ; 
 
 	 / / r e t u r n s   f r o m   u r l 
 
 	 s t r i n g   r e t u r n S t r i n g ; 
 
 	 i n t   r e t u r n I n t ; 
 
 	 f l o a t   r e t u r n F l o a t ; 
 
 	 / / p u b l i c   P l a y e r N e t w o r k S e t u p   s e t u p B o x ; 
 
 	 p u b l i c   G a m e M a n a g e r   g a m e M a n a g e r ; 
 
 	 p u b l i c   C o i n M a n a g e r   c o i n M a n a g e r ; 
 
 
 
 	 p u b l i c   i n t   b o x C o u n t ; 
 
 	 p u b l i c   T e x t   c a n v a s T e x t ; 
 
 
 
 	 [ S y n c V a r ]   p u b l i c   s t r i n g   m e s s a g e   =   " " ; 
 
 	 [ S y n c V a r ]   p u b l i c   s t r i n g   r e s u l t M e s s a g e   =   " " ; 
 
 	 s t r i n g   o l d M e s s a g e ; 
 
 
 
 	 / / s e t   i n   p l a y e r n e t w o r k s e t u p   t o   c r e a t e   n e t w o r k   a u t h o r i t y 
 
 	 p u b l i c   P a r t i c i p a n t C o n t r o l l e r   p a r t i c i p a n t C o n t r o l l e r ; 
 
 	 p u b l i c   b o o l   _ i s L o c a l P l a y e r ; 
 
 	 / / S h a r e d   f r o m   h o s t 
 
 	 / / [ H i d e I n I n s p e c t o r ] 
 
 
 
 	 [ S y n c V a r ]   p u b l i c   i n t   s t a g e _ n u m b e r   =   0 ; 
 
 	 [ S y n c V a r ] p u b l i c   r u n S t a t e   m o d e   =   r u n S t a t e . w a i t ; 
 
 	 / / s t a g e   n u m b e r   w h e n   r e s u l t   r e c o r d e d 
 
 	 i n t   r e s u l t S t a g e   =   0 ; 
 
 	 p u b l i c   E x p e r i m e n t C o n t r o l l e r [ ]   t o k e n B o x e s ; 
 
 	 I K B o d y   i k B o d y   =   n u l l ; 
 
 	 / / f i x m e 
 
 	 s t r i n g   r e s u l t ; 
 
 	 b o o l   s t a r t ; 
 
 	 / /   U s e   t h i s   f o r   i n i t i a l i z a t i o n 
 
 	 v o i d   S t a r t   ( ) 
 
 	 { 
 
 	 	 / / s t a r t   a t   s t a g e   0 
 
 
 
 	 	 u r l R e t u r n   =   t r u e ; 
 
 	 	 u p d a t e   =   t r u e ; 
 
 
 
 	 	 c o i n M a n a g e r   =   ( C o i n M a n a g e r ) G e t C o m p o n e n t < C o i n M a n a g e r >   ( ) ; 
 
 
 
 	 	 / / r e d u c e   e r r o r   i n   s c e n e   s e t u p 
 
 	 	 g a m e M a n a g e r   =   G a m e O b j e c t . F i n d   ( " N e t w o r k M a n a g e r " ) . G e t C o m p o n e n t < G a m e M a n a g e r >   ( ) ; 
 
 	 	 b o x C o u n t   =   g a m e M a n a g e r . b o x C o u n t ; 
 
 	 	 t e x t F i l e R e a d e r   =   G a m e O b j e c t . F i n d   ( " N e t w o r k M a n a g e r " ) . G e t C o m p o n e n t < T e x t F i l e R e a d e r >   ( ) ; 
 
 	 	 b u t t o n   =   t r a n s f o r m . F i n d   ( " C a p s u l e " ) ; 
 
 	 
 
 	 	 s t a r t   =   t r u e ; 
 
 
 
 	 } 
 
 
 
 	 v o i d   s e t u p H o s t   ( ) 
 
 	 { 
 
 
 
 
 
 	 	 i f   ( i s H o s t   & &   t o k e n B o x e s . L e n g t h   = =   0 )   { 
 
 	 	 	 L i s t   < E x p e r i m e n t C o n t r o l l e r >   l i s t   =   n e w   L i s t < E x p e r i m e n t C o n t r o l l e r >   ( ) ; 
 
 	 	 	 f o r e a c h   ( G a m e O b j e c t   b o x   i n   g a m e M a n a g e r . t o k e n B o x e s )   { 
 
 
 
 	 	 	 	 l i s t . A d d   ( b o x . G e t C o m p o n e n t < E x p e r i m e n t C o n t r o l l e r >   ( ) ) ; 
 
 	 	 	 } 
 
 	 	 	 t o k e n B o x e s   =   l i s t . T o A r r a y   ( ) ; 
 
 
 
 	 	 	 / / D e b u g . L o g ( g a m e M a n a g e r ) ; 
 
 	 	 } 
 
 	 	 s t a r t   =   f a l s e ; 
 
 	 } 
 
 
 
 	 v o i d   U p d a t e   ( ) 
 
 	 { 
 
 	 	 / / w a i t   f o r   u p d a t e s   f r o m   a p i 
 
 	 	 r o u n d _ i d   =   g a m e M a n a g e r . r o u n d _ i d ; 
 
 	 	 i f   ( u r l R e t u r n )   { 
 
 	 	 	 / /   u r l   c a l l s   i n   r e s t   o f   u p d a t e   d o   n o t   w o r k 
 
 	 	 	 i f   ( i s H o s t   & &   _ i s L o c a l P l a y e r )   { 
 
 	 	 	 	 / / f i n d   n e x t   s t e p   a n d   m e s s a g e 
 
 	 	 	 	 u p d a t e M o v e   ( ) ; 
 
 	 	 	 	 / / m e s s a g e   e t c   i s   s e n d   o n   C o m m a n d   o n   s e r v e r   a n d   t o   a l l   p l a y e r s 
 
 	 	 	 	 / / t h e   s y n c v a r   t o   l o c a l p l a y e r 
 
 	 	 	 } 
 
 
 
 
 
 	 	 	 / / n o t   w o r k i n g   i n   s t a r t   u n l e s s   s e t   t o   a c t i v e   l a t e r     a s   n o   t o k e n b o x -   t h e n   n o t   v i s i b l e   f o r   c o l l e c t i o n   t o k e n b o x e s   F I X M E 
 
 	 	 	 i f   ( s t a r t ) 
 
 	 	 	 	 s e t u p H o s t   ( ) ; 
 
 	 
 
 
 
 	 	 	 i f   ( _ i s L o c a l P l a y e r )   { 
 
 	 	 	 	 / / s e t   f r o m   t h e   s e r v e r   s y n c v a r 
 
 
 
 	 	 	 	 i f   ( i k B o d y   ! =   n u l l   &   c o i n M a n a g e r . p l a y e r   ! =   n u l l ) 
 
 	 	 	 	 	 i k B o d y . i k A c t i v e   =   i k A c t i v e ; 
 
 	 	 	 	 e l s e 
 
 	 	 	 	 	 / / g e t   c o m p o n e n t   i f   n u l l 
 
 	 	 	 	 	 i k B o d y   =   c o i n M a n a g e r . p l a y e r . g a m e O b j e c t . G e t C o m p o n e n t < I K B o d y >   ( ) ; 
 
 	 	 	 
 
 	 	 	 	 s w i t c h   ( m o d e )   { 
 
 	 	 	 	 c a s e   r u n S t a t e . s t a r t : 
 
 	 	 	 	 	 / / s h o w   t h i s   a t   s t a r t 
 
 
 
 	 	 	 	 	 c o i n M a n a g e r . r e s u l t   =   f a l s e ; 
 
 	 	 	 	 	 c a n v a s T e x t . t e x t   =   " W a i t   f o r   o t h e r s   t o   j o i n   y o u " ; 
 
 	 	 	 
 
 	 	 	 	 	 b r e a k ; 
 
 
 
 	 	 	 	 c a s e   r u n S t a t e . w a i t : 
 
 	 	 	 	 	 / / w a i t   f o r   n e x t   s t e p   -   n e e d   s i g n a l   t h a t   a l l   h a v e   d o n e   i t   f r o m   a p i   -   F I X M E ? 
 
 
 
 	 	 	 	 	 i f   ( c o i n M a n a g e r . i s F i n i s h e d )   { 
 
 	 	 	 	 	 	 w a i t i n g   =   t r u e ; 
 
 	 	 	 	 	 	 c o i n M a n a g e r . i s F i n i s h e d   =   f a l s e ; 
 
 	 	 	 	 	 } 
 
 	 	 	 	 	 / / d o n ' t   c h a n g e   m e s s a g e   s o   n o t   r e w r i t t e n 
 
 	 	 	 	 	 b r e a k ; 
 
 	 	 	 	 c a s e   r u n S t a t e . a s k : 
 
 	 	 	 	 	 / / e n a b l e   i k   a n d   s y n c   f r o m   p a r t i c i p a n t c o n t r o l l e r   w h e n   s i t t i n g 
 
 	 	 	 	 	 / / i k A c t i v e   =   t r u e ; 
 
 	 	 	 	 	 / / s e n d   t o   s e r v e r   t h e n   S y n c V a r 
 
 	 	 	 	 	 i f   ( i k A c t i v e )   { 
 
 	 	 	 	 	 	 c o i n M a n a g e r . p l a y e r . C m d _ i k A c t i v e   ( b o x C o u n t ,   t r u e ) ; 
 
 
 
 	 	 	 	 	 	 / / s e t   u p   t o   c h a n g e   c o i n s 
 
 	 	 	 	 	 	 c o i n M a n a g e r . _ i s L o c a l P l a y e r   =   t r u e ; 
 
 	 	 	 	 	 	 	 
 
 	 	 	 	 	 	 V e c t o r 3   t a r g e t   =   b u t t o n . t r a n s f o r m . p o s i t i o n ; 
 
 	 	 	 	 	 	 l e f t h a n d E f f e c t o r . r o t a t i o n   =   c o i n M a n a g e r . g a m e O b j e c t . t r a n s f o r m . p a r e n t . r o t a t i o n   *   Q u a t e r n i o n . E u l e r   ( - 1 8 f ,   - 1 5 f ,   4 0 f ) ; 
 
 	 	 	 	 	 	 / / n e e d   t o   a d d   r o t a t i o n   o f   c h a i r 
 
 
 
 	 	 	 	 	 	 l e f t h a n d E f f e c t o r . t r a n s f o r m . p o s i t i o n   =   t a r g e t ; 
 
 	 	 	 	 	 	 b u t t o n . G e t C o m p o n e n t < C l e a r B u t t o n >   ( ) . _ i s L o c a l P l a y e r   =   t r u e ; 
 
 	 	 	 	 	 	 / / s e n d   c o i n   n u m b e r 	 
 
 	 	 	 	 	 	 c a n v a s T e x t . t e x t   =   m e s s a g e   +   "   Y o u   h a v e   s e l e c t e d   "   +   c o i n M a n a g e r . c u r r e n t C o i n s   +   "   c o i n s " ; 
 
 	 	 	 	 	 	 i f   ( c o i n M a n a g e r . i s F i n i s h e d   &   _ i s L o c a l P l a y e r )   { 
 
 	 	 	 	 	 	 	 r o u n d _ i d   =   g a m e M a n a g e r . r o u n d _ i d ; 
 
 	 	 	 	 	 	 	 / / c a n n o t   e n t e r   a n y m o r e 
 
 	 	 	 	 	 	 	 e f f o r t C o i n s   =   c o i n M a n a g e r . c u r r e n t C o i n s ; 
 
 	 	 	 	 	 	 	 b u t t o n . G e t C o m p o n e n t < C l e a r B u t t o n >   ( ) . _ i s L o c a l P l a y e r   =   f a l s e ; 
 
 	 	 	 	 	 	 	 r e s u l t S t a g e   =   s t a g e _ n u m b e r ; 
 
 	 	 	 	 	 	 	 / / s e n d   i n   r e s u l t   t o   Z T r e e 
 
 	 	 	 	 	 	 	 c a n v a s T e x t . t e x t   =   " W a i t   f o r   o t h e r s   t o   f i n i s h " ; 
 
 
 
 	 	 	 	 	 	 	 / / e n t e r   r e s u l t 
 
 	 	 	 	 	 	 	 u r l   =   t e x t F i l e R e a d e r . I P _ A d d r e s s   +   " / e x p e r i m e n t s / r e s u l t s ? e x p e r i m e n t _ i d = "   +   t e x t F i l e R e a d e r . e x p e r i m e n t _ i d   +   " & s t a g e _ n u m b e r = "   +   s t a g e _ n u m b e r   +   " & p a r t i c i p a n t _ i d = "   +   p a r t i c i p a n t _ i d   +   " & r o u n d _ i d = " + r o u n d _ i d . T o S t r i n g ( ) + " & n a m e = C o i n E f f o r t & v a l u e = "   +   c o i n M a n a g e r . c u r r e n t C o i n s ; 
 
 	 	 	 	 	 	 	 S t a r t C o r o u t i n e   ( F e t c h S t a g e   ( u r l ,   " " ,   " " ,   m o d e ) ) ; 
 
 	 	 	 	 	 	 	 / / n o t   p l a y i n g   a n y m o r e 
 
 	 	 	 	 	 	 	 i k A c t i v e   =   f a l s e ; 
 
 	 	 	 	 	 	 	 c o i n M a n a g e r . p l a y e r . C m d _ i k A c t i v e   ( b o x C o u n t ,   f a l s e ) ; 
 
 	 	 	 	 	 	 	 u r l   =   " " ; 
 
 	 	 	 	 	 	 	 m o d e   =   r u n S t a t e . w a i t ; 
 
 	 	 	 	 	 	 	 / / c a n n o t   u s e   b u t t o n 
 
 	 	 	 	 	 	 	 b u t t o n . G e t C o m p o n e n t < C l e a r B u t t o n >   ( ) . S e t T o C l e a r   ( f a l s e ) ; 
 
 	 	 	 	 	 	 } 
 
 	 	 	 	 	 } 
 
 	 	 	 	 	 b r e a k ; 
 
 
 
 	 	 	 	 c a s e   r u n S t a t e . a n s w e r : 
 
 	 	 	 	 	 / / c a l l   f o r   r e s u l t   o n c e   p e r   p a r t i c i p a n t 
 
 	 	 	 	 	 i f   ( u p d a t e )   { 
 
 	 	 	 	 	 	 / / g e t   r e s u l t   f o r   p r e v i o u s   s t a g e   f o r   e a c h   p a r t i c i p a n t 
 
 	 	 	 	 	 	 u r l   =   t e x t F i l e R e a d e r . I P _ A d d r e s s   +   " / e x p e r i m e n t s / r e s u l t s ? e x p e r i m e n t _ i d = "   +   t e x t F i l e R e a d e r . e x p e r i m e n t _ i d   +   " & s t a g e _ n u m b e r = "   +   ( r e s u l t S t a g e )   +   " & r o u n d _ i d = "   +   r o u n d _ i d . T o S t r i n g   ( )   +   " & n a m e = R e s u l t & p a r t i c i p a n t _ i d = "   +   p a r t i c i p a n t _ i d ; 
 
 	 	 	 	 	 	 / / g e t s   r e s u l t   a n d   d i s p l a y s   t o   l o c a l   c a n v a s T e x t . t e x t 
 
 	 	 	 	 	 	 S t a r t C o r o u t i n e   ( F e t c h S t a g e   ( u r l ,   " R e s u l t s " ,   " " ,   m o d e ) ) ; 
 
 	 	 	 	 	 	 / / h a s   w a i t   a t   e n d   t o   s t o p   g o i n g   t o   e n d   s c r e e n   t o o   q u i c k 
 
 	 	 	 	 	 	 u r l   =   " " ; 
 
 	 	 	 	 	 	 u p d a t e   =   f a l s e ; 
 
 	 	 	 	 	 	 / / F I X M E   s h o u l d   g o   t o   w a i t ,   b u t   g e t   m e s s a g e   c h a n g e 
 
 	 	 	 	 	 	 / / m o d e   =   r u n S t a t e . w a i t ; 
 
 	 	 	 	 	 } 
 
 
 
 	 	 	 	 	 b r e a k ; 
 
 	 	 	 	 c a s e   r u n S t a t e . e n d : 
 
 	 	 	 	 	 
 
 	 	 	 	 
 
 	 	 	 	 
 
 	 	 	 	 	 p a r t i c i p a n t C o n t r o l l e r . m o d e   =   P a r t i c i p a n t C o n t r o l l e r . m o d e s . s t a n d ; 
 
 	 	 	 	 	 / / g a m e M a n a g e r . b o x C o u n t   =   - 1 ; 
 
 
 
 
 
 	 	 	 	 	 b r e a k ; 
 
 	 	 	 	 } 
 
 	 	 	 	 / / D e b u g . L o g W a r n i n g   ( m e s s a g e ) ; 
 
 	 	 	 } 
 
 
 
 	 	 	 / / w o r k s   w h e n   s y n c v a r   g i v e s   n e w   b r a o d c a s t   m e s s a g e   t o   p l a y e r 
 
 	 	 
 
 	 	 	 i f   ( _ i s L o c a l P l a y e r   &   o l d M e s s a g e   ! =   m e s s a g e   &   ! m e s s a g e . E q u a l s   ( " " ) )   { 
 
 	 	 	 	 s h o w M e s s a g e   ( m e s s a g e ) ; 
 
 	 	 	 	 o l d M e s s a g e   =   m e s s a g e ; 
 
 	 	 	 } 
 
 	 	 
 
 	 	 } 
 
 
 
 	 } 
 
 
 
 	 / / I E n u m e r a t o r   r e s u l t M e s s a g e ( s t r i n g   _ m e s s a g e ) { 
 
 	 / / m a k e   s u r e   s e e   r e t u r n   m e s s a g e   b e f o r e   f i n a l   r e s u l t 
 
 	 / / 	 y i e l d   r e t u r n   S t a r t C o r o u t i n e   ( W a i t F o r S e c o n d s   ( . 1 f ) ) ; 
 
 	 / / 	 c a n v a s T e x t . t e x t   =   _ m e s s a g e ; 
 
 	 / / r e s e t   i n   c l a s s 
 
 	 / / 	 m e s s a g e   =   " " ; 
 
 
 
 	 / / } 
 
 
 
 
 
 	 / / d e a l s   w i t h   g r o u p   m e s s a g e s ,   n o t   i n d i v i d u a l   o n e s 
 
 	 / / t h e y   a r e   d o n e   u n d e r   m o d e s 
 
 	 v o i d   s h o w M e s s a g e   ( s t r i n g   _ m e s s a g e ) 
 
 	 { 
 
 	 	 / / u p d a t e   i f   n o t   b l a n k 
 
 	 	 i f   ( ! _ m e s s a g e . E q u a l s   ( " " ) ) 
 
 	 	 	 c a n v a s T e x t . t e x t   =   _ m e s s a g e ; 
 
 
 
 	 } 
 
 
 
 	 v o i d   u p d a t e M o v e   ( ) 
 
 	 { 
 
 	 	 / / r u n s   o n   h o s t   l o c a l p l a y e r   o n l y 
 
 	 	 / / d o   n o t   b r o a d c a s t   u n l e s s   c h a n g e   a s   s o m e   a r e   i n   w a i t i n g 
 
 	 	 i n t   _ s t a g e _ n u m b e r   =   s t a g e _ n u m b e r ; 
 
 	 
 
 	 	 s t r i n g   _ m e s s a g e   =   m e s s a g e ; 
 
 	 / / r e t u r n e d   d a t a   f r o m   u r l 
 
 	 	 i f   ( u r l R e t u r n )   { 
 
 
 
 	 	 	 i f   ( m o d e   ! =   r u n S t a t e . e n d )   { 
 
 	 	 	 	 
 
 	 	 	 	 u r l   =   t e x t F i l e R e a d e r . I P _ A d d r e s s   +   " / e x p e r i m e n t s / s t a g e s ? e x p e r i m e n t _ i d = "   +   t e x t F i l e R e a d e r . e x p e r i m e n t _ i d   +   " & s t a g e _ n u m b e r = "   +   s t a g e _ n u m b e r   +   " & r o u n d _ i d = "   +   r o u n d _ i d . T o S t r i n g   ( ) ; 
 
 	 	 	 	 / / D e b u g . L o g   ( u r l ) ; 
 
 
 
 	 	 	 	 S t a r t C o r o u t i n e   ( F e t c h S t a g e   ( u r l ,   " t y p e _ s t a g e " ,   " s t a g e _ n u m b e r " ,   m o d e ) ) ; 
 
 	 	 	 	 u r l   =   " " ; 
 
 
 
 	 	 	 	 / / m o v e   o n   t o   n e x t   o n e 
 
 	 	 	 	 i f   ( r e t u r n I n t   >   0 ) 
 
 	 	 	 	 	 s t a g e _ n u m b e r   =   r e t u r n I n t ; 
 
 	 	 	 	 
 
 	 	 	 	 i f   ( r e t u r n S t r i n g   = =   " R e q u e s t " )   { 
 
 	 	 	 	 	 
 
 
 
 	 	 	 	 	 m o d e   =   r u n S t a t e . a s k ; 
 
 
 
 	 	 	 	 }   e l s e   i f   ( r e t u r n S t r i n g   = =   " R e s p o n s e " )   { 
 
 	 	 	 	 	 
 
 
 
 	 	 	 	 	 m o d e   =   r u n S t a t e . a n s w e r ; 
 
 
 
 
 
 	 	 	 	 }   e l s e   i f   ( r e t u r n S t r i n g   = =   " W a i t " )   { 
 
 
 
 	 	 	 	 	 m o d e   =   r u n S t a t e . w a i t ; 
 
 
 
 	 	 	 	 }   e l s e   i f   ( r e t u r n S t r i n g   = =   " E n d " )   { 
 
 
 
 	 	 	 	 	 m o d e   =   r u n S t a t e . e n d ; 
 
 
 
 	 	 	 	 } 
 
 
 
 	 	 	 } 
 
 
 
 
 
 	 	 	 t r y   { 
 
 
 
 	 	 	 	 / / f i x M e   i s   t h i s   a   p r o b l e m ? ? 
 
 	 	 	 	 i f   ( _ s t a g e _ n u m b e r   ! =   s t a g e _ n u m b e r )   { 
 
 	 	 	 	 	 c o i n M a n a g e r . p l a y e r . C m d _ c h a n g e _ c u r r e n t S t a g e   ( s t a g e _ n u m b e r ,   m o d e ) ; 
 
 	 	 	 	 
 
 	 	 	 	 } 
 
 	 	 	 }   c a t c h   { 
 
 	 	 	 } 
 
 
 
 
 
 	 	 
 
 
 
 	 	 	 i f   ( m e s s a g e   ! =   _ m e s s a g e )   { 
 
 	 	 	 	 / / s e n d   u p d a t e   o f   r e s u l t   M e s s a g e   t o o   f o r   w h e n   i t   c o m e s   i n 
 
 	 	 	 	 / / e m p y   m e s s a g e   n o t   d i s p l a y e d 
 
 	 	 	 	 c o i n M a n a g e r . p l a y e r . C m d _ b r o a d c a s t   ( m e s s a g e ,   r e s u l t M e s s a g e ) ; 
 
 
 
 	 	 	 } 
 
 	 	 } 
 
 	 } 
 
 
 
 
 
 
 
 
 
 	 I E n u m e r a t o r   F e t c h S t a g e   ( s t r i n g   _ u r l ,   s t r i n g   f i n d ,   s t r i n g   f i n d I n t ,   r u n S t a t e   _ m o d e ) 
 
 	 { 
 
 	 	 i f   ( _ i s L o c a l P l a y e r )   { 
 
 	 	 	 u r l R e t u r n   =   f a l s e ; 
 
 	 	 	 / / D e b u g . L o g W a r n i n g   ( u r l ) ; 
 
 
 
 	 	 	 y i e l d   r e t u r n   S t a r t C o r o u t i n e   ( W a i t F o r S e c o n d s   ( . 5 f ) ) ; 
 
 	 	 	 W W W   w w w   =   n e w   W W W   ( _ u r l ) ; 
 
 
 
 	 	 	 y i e l d   r e t u r n   S t a r t C o r o u t i n e   ( W a i t F o r R e q u e s t   ( w w w ) ) ; 
 
 	 	 	 / / g o   t o   n e x t   s t e p   w h e n   d o n e 
 
 	 	 	 u r l R e t u r n   =   t r u e ; 
 
 	 	 	 / /   S t r i n g B u i l d e r   s b   =   n e w   S t r i n g B u i l d e r ( ) ; 
 
 	 	 	 s t r i n g   r e s u l t   =   w w w . t e x t ; 
 
 	 	 	 J S O N N o d e   n o d e   =   J S O N . P a r s e   ( r e s u l t ) ; 
 
 	 	 	 
 
 	 	 	 i f   ( n o d e   ! =   n u l l )   { 
 
 	 	 	 	 t r y   { 
 
 	 	 	 	 	 / / g e t   s t a g e   m e s s a g e 
 
 	 	 	 	 	 m e s s a g e   =   n o d e   [ " m e s s a g e " ] ; 
 
 	 	 	 	 	 i f   ( n o d e   [ " t y p e _ s t a g e " ]   = =   " E n d " )   { 
 
 	 	 	 	 	 	 r e s u l t M e s s a g e   =   m e s s a g e ; 
 
 	 	 	 	 	 } 
 
 	 	 	 	 }   c a t c h   { 
 
 	 	 	 	 	 / / m e s s a g e   =   n u l l ; 
 
 	 	 	 	 	 / / y i e l d   r e t u r n   f a l s e ; 
 
 	 	 	 	 } 
 
 	 	 
 
 	 	 	 	 / / D e b u g . L o g   ( m e s s a g e ) ; 
 
 	 	 
 
 	 	 	 	 i f   ( f i n d . L e n g t h   ! =   0 )   { 
 
 
 
 	 	 	 	 	 r e t u r n S t r i n g   =   n o d e   [ f i n d ] ; 
 
 	 	 	 	 	 r e t u r n F l o a t   =   0 ; 
 
 	 	 	 	 	 / / 	 D e b u g . L o g W a r n i n g   ( n o d e ) ; 
 
 	 	 	 	 	 i f   ( f i n d   = =   " R e s u l t s " )   { 
 
 	 	 	 	 	 	 / / h a c k   t o   g e t   r e s u l t s   i n t o   m e s s a g e -   t h e   t i m e   d e l a y 
 
 	 	 	 	 	 	 / / m e n s   y o u   c a n n o t   p i c k   t h i s   u p   i n   t h e   s t a t e   m a c h i n e 
 
 
 
 
 
 	 	 	 	 	 	 i f   ( f l o a t . T r y P a r s e   ( r e t u r n S t r i n g ,   o u t   r e t u r n F l o a t ) )   { 
 
 	 	 	 	 	 	 	 / / g e t   b a c k   r e s u l t   f r o m   g r o u p 
 
 	 	 	 	 	 	 	 / / w h e n   g e t   r e s u l t   s h o w   i t ` 
 
 	 	 	 	 	 
 
 
 
 	 	 	 	 	 	 	 / / F I X M E 
 
 	 	 	 	 	 	 	 i f   ( r e t u r n F l o a t   >   0   & &   ! m e s s a g e . E q u a l s   ( " " ) )   { 
 
 	 	 	 	 	 	 	 	 / / s e t   t o   d i s p l a y   r e s u l t   o n l y 
 
 	 	 	 	 	 	 	 	 r e s u l t C o i n s   = 	 c o i n M a n a g e r . m a x C o i n s   +   1   -   e f f o r t C o i n s   +   ( i n t ) r e t u r n F l o a t ; 
 
 	 	 	 	 	 	 	 	 c o i n M a n a g e r . r e s u l t   =   t r u e ; 
 
 	 	 	 	 	 	 
 
 	 	 	 	 	 	 	 	 c o i n M a n a g e r . c u r r e n t C o i n s   - =   ( i n t ) r e t u r n F l o a t ; 
 
 
 
 	 	 	 	 	 
 
 	 	 	 	 	 	 
 
 	 	 	 	 	 	 
 
 	 	 	 	 	 	 	 	 / / d i s p l a y   r e s u l t s   -   n o   e n t e r e d   c o i n s   s h o w   a n y m o r e   -   f i x i t 
 
 
 
 	 	 	 	 	 	 	 	 c a n v a s T e x t . t e x t   =   m e s s a g e   +   r e t u r n F l o a t . T o S t r i n g   ( ) ; 
 
 	 	 	 	 	 	 	 	 / / 	 D e b u g . L o g W a r n i n g   ( m e s s a g e   +   r e t u r n F l o a t . T o S t r i n g   ( ) ) ; 
 
 	 	 	 	 	 	 	 	 / / s t o p   b r o a d c a s t 
 
 	 	 	 	 	 	 	 	 m e s s a g e = " " ; 
 
 	 	 	 	 	 	 	 	 y i e l d   r e t u r n   S t a r t C o r o u t i n e   ( W a i t F o r S e c o n d s   ( . 5 f ) ) ; 
 
 	 	 	 	 	 	 	 	 / / d e l a y   d i s p l a y   o f   f i n a l   m e s s a g e 
 
 	 	 	 	 	 	 	 	 / / s t o p   b r o a d c a s t 
 
 	 	 	 	 	 	 	 	 m e s s a g e = " " ; 
 
 	 	 	 	 	 	 	 	 i f ( r e s u l t M e s s a g e ! = " " ) c a n v a s T e x t . t e x t   =   r e s u l t M e s s a g e   +   r e s u l t C o i n s . T o S t r i n g   ( ) ; 
 
 	 	 	 	 	 	 	 	 r e s u l t M e s s a g e   =   " " ; 
 
 	 	 	 	 	 	 	 } 
 
 	 	 	 	 	 	 	 
 
 	 	 	 	 	 	 	 y i e l d   r e t u r n   t r u e ; 
 
 	 	 	 	 	 
 
 	 	 	 	 	 	 	 / / m e s s a g e   f o r   l o c a l p l a y e r / t o k e n b o x   o n l y 
 
 	 	 	 	 	 	 } 
 
 
 
 	 	 	 	 	 	 y i e l d   r e t u r n   t r u e ; 
 
 	 	 	 	 	 }   e l s e   i f   ( I n t 3 2 . T r y P a r s e   ( n o d e   [ f i n d I n t ] ,   o u t   r e t u r n I n t ) )   { 
 
 	 	 	 	 	 
 
 	 	 	 	 	 	 / / D e b u g . L o g ( r e t u r n I n t ) ; 
 
 	 	 	 	 	 	 y i e l d   r e t u r n   t r u e ; 
 
 	 	 	 	 	 } 
 
 
 
 	 	 	 	 	 y i e l d     r e t u r n   t r u e ; 
 
 	 	 	 	 }   e l s e   { 
 
 
 
 	 	 	 	 	 i f   ( I n t 3 2 . T r y P a r s e   ( n o d e   [ f i n d I n t ] ,   o u t   r e t u r n I n t ) ) 
 
 	 	 	 	 	 	 y i e l d   r e t u r n   t r u e ; 
 
 	 	 	 	 
 
 	 	 	 	 } 
 
 	 	 	 }   e l s e   { 
 
 	 	 	 	 / / D e b u g . L o g W a r n i n g   ( " N o   n o d e   o n   a p i   r e a d   f o r   "   +   f i n d   +   "   o r   "   +   f i n d I n t ) ; 
 
 	 	 	 	 / / c a n v a s . m e s s a g e   =   " E r r e r   i n   s t a g e s   f o r   e x p e r i m e n t :   "   +   n o d e ; 
 
 	 	 	 	 y i e l d   r e t u r n   t r u e ; 
 
 
 
 	 	 	 } 
 
 	 	 } 
 
 	 	 y i e l d   b r e a k ; 
 
 	 } 
 
 
 
 	 I E n u m e r a t o r   s e t u p W a i t   ( f l o a t   n u m ) 
 
 	 { 
 
 	 	 y i e l d   r e t u r n   W a i t F o r S e c o n d s   ( n u m ) ; 
 
 
 
 
 
 	 } 
 
 
 
 	 p u b l i c   I E n u m e r a t o r   W a i t F o r R e q u e s t   ( W W W   w w w ) 
 
 	 { 
 
 
 
 	 	 y i e l d   r e t u r n   w w w ; 
 
 
 
 	 } 
 
 
 
 	 I E n u m e r a t o r   W a i t F o r S e c o n d s   ( f l o a t   n u m ) 
 
 	 { 
 
 	 	 
 
 	 	 y i e l d   r e t u r n   n e w   W a i t F o r S e c o n d s   ( n u m ) ; 
 
 
 
 	 } 
 
 } 