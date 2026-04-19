import api from './api';

export interface CartItem {
  id: number;
  productId: number;
  name: string;
  price: number;
  quantity: number;
  supplier: string;
}

export interface Cart {
  id: number;
  userId: number;
  totalPrice: number;
  items: CartItem[];
}

export const cartService = {
  getCart: async (userId: number): Promise<Cart> => {
    const response = await api.get<Cart>(`/api/cart/${userId}`);
    return response.data;
  },
  
  addToCart: async (userId: number, productId: number, quantity: number): Promise<{ message: string }> => {
    const response = await api.post<{ message: string }>(`/api/cart/${userId}/items`, { productId, quantity });
    return response.data;
  },

  removeItem: async (userId: number, cartItemId: number): Promise<void> => {
    await api.delete(`/api/cart/${userId}/items/${cartItemId}`);
  },

  // THE FIX: Bulk checkout endpoint for the cart
  checkoutCart: async (userId: number, cartItemIds: number[]): Promise<{ message: string, externalReference: string, status: number }> => {
    const response = await api.post(`/api/order/checkout/cart`, { userId, cartItemIds });
    return response.data;
  }
};